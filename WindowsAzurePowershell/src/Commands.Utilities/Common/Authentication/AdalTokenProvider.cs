// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using IdentityModel.Clients.ActiveDirectory;
    using Properties;

    /// <summary>
    /// A token provider that uses ADAL to retrieve
    /// tokens from Azure Active Directory
    /// </summary>
    public class AdalTokenProvider : ITokenProvider
    {
        private readonly IDictionary<TokenCacheKey, string> tokenCache;
        private readonly IWin32Window parentWindow;

        public AdalTokenProvider()
            : this(new ConsoleParentWindow(), new AdalRegistryTokenCache())
        {
        }

        public AdalTokenProvider(IWin32Window parentWindow, IDictionary<TokenCacheKey, string> tokenCache)
        {
            this.parentWindow = parentWindow;
            this.tokenCache = tokenCache;
        }

        public IAccessToken GetNewToken(WindowsAzureSubscription subscription, string userId)
        {
            var config = new AdalConfiguration(subscription);
            return new AdalAccessToken(AcquireToken(config, userId), this, config);
        }

        public IAccessToken GetNewToken(WindowsAzureEnvironment environment)
        {
            var config = new AdalConfiguration(environment);
            return new AdalAccessToken(AcquireToken(config), this, config);
        }

        public IAccessToken GetCachedToken(WindowsAzureSubscription subscription, string userId)
        {
            var key = tokenCache.Keys.FirstOrDefault(k => k.UserId == userId && k.TenantId == subscription.ActiveDirectoryTenantId);
            if (key == null)
            {
                throw new AadAuthenticationFailedException(string.Format(Resources.NoCachedToken,
                    subscription.SubscriptionName, userId));
            }

            return new AdalAccessToken(DecodeCachedAuthResult(key), this, new AdalConfiguration(subscription));
        }

        /// <summary>
        /// Decode cache contents into something we can feed to AuthenticationResult.Deserialize.
        /// WARNING: This will be deprecated eventually by ADAL team and replaced by something supported.
        /// </summary>
        /// <param name="key">The cache key for the entry to decode</param>
        /// <returns>The decoded string to pass to AuthenticationResult.Deserialize</returns>
        private AuthenticationResult DecodeCachedAuthResult(TokenCacheKey key)
        {
            string encoded = tokenCache[key];
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            return AuthenticationResult.Deserialize(decoded);
        }

        private readonly static TimeSpan thresholdExpiration = new TimeSpan(0, 5, 0);

        private bool IsExpired(AdalAccessToken token)
        {
#if DEBUG
            if (Environment.GetEnvironmentVariable("FORCE_EXPIRED_ACCESS_TOKEN") != null)
            {
                return true;
            }
#endif

            return token.AuthResult.ExpiresOn - DateTimeOffset.Now < thresholdExpiration;
        }

        private string GetRefreshToken(AdalAccessToken token)
        {
#if DEBUG
            if (Environment.GetEnvironmentVariable("FORCE_EXPIRED_REFRESH_TOKEN") != null)
            {
                // We can't force an expired refresh token, so provide a garbage one instead
                const string fakeToken = "This is not a valid refresh token";
                return Convert.ToBase64String(Encoding.ASCII.GetBytes(fakeToken));
            }
#endif
            return token.AuthResult.RefreshToken;
        }

        private void Renew(AdalAccessToken token)
        {
            if (IsExpired(token))
            {
                var context = CreateContext(token.Configuration);
                try
                {
                    var authResult = context.AcquireTokenByRefreshToken(GetRefreshToken(token),
                        token.Configuration.ClientId,
                        token.Configuration.ResourceClientUri);
                    if (authResult == null)
                    {
                        throw new Exception(Resources.ExpiredRefreshToken);
                    }
                    token.AuthResult = authResult;
                }
                catch (Exception ex)
                {
                    throw new AadAuthenticationCantRenewException(Resources.ExpiredRefreshToken, ex);
                }
            }
        }

        private AuthenticationContext CreateContext(AdalConfiguration config)
        {
            return new AuthenticationContext(config.AdEndpoint + config.AdDomain, config.ValidateAuthority, tokenCache)
            {
                OwnerWindow = parentWindow
            };
        }

        // We have to run this in a separate thread to guarantee that it's STA. This method
        // handles the threading details.
        private AuthenticationResult AcquireToken(AdalConfiguration config, string userId = null)
        {
            AuthenticationResult result = null;
            Exception ex = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var context = CreateContext(config);
                    if (string.IsNullOrEmpty(userId))
                    {
                        ClearCookies();
                        result = context.AcquireToken(config.ResourceClientUri, config.ClientId,
                            config.ClientRedirectUri, PromptBehavior.Always, AdalConfiguration.EnableEbdMagicCookie);
                    }
                    else
                    {
                        result = context.AcquireToken(config.ResourceClientUri, config.ClientId,
                            config.ClientRedirectUri, userId, AdalConfiguration.EnableEbdMagicCookie);
                    }
                }
                catch (Exception threadEx)
                {
                    ex = threadEx;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AcquireTokenThread";
            thread.Start();
            thread.Join();
            if (ex != null)
            {
                var adex = ex as ActiveDirectoryAuthenticationException;
                if (adex != null)
                {
                    if (adex.ErrorCode == ActiveDirectoryAuthenticationError.AuthenticationCanceled)
                    {
                        throw new AadAuthenticationCanceledException(adex.Message, adex);
                    }
                }
                throw new AadAuthenticationFailedException(GetExceptionMessage(ex), ex);
            }

            return result;
        }

        private string GetExceptionMessage(Exception ex)
        {
            string message = ex.Message;
            if (ex.InnerException != null)
            {
                message += ": " + ex.InnerException.Message;
            }
            return message;
        }
        /// <summary>
        /// Implementation of <see cref="IAccessToken"/> using data from ADAL
        /// </summary>
        private class AdalAccessToken : IAccessToken
        {
            internal readonly AdalConfiguration Configuration;
            internal AuthenticationResult AuthResult;
            private readonly AdalTokenProvider tokenProvider;

            public AdalAccessToken(AuthenticationResult authResult, AdalTokenProvider tokenProvider, AdalConfiguration configuration)
            {
                AuthResult = authResult;
                this.tokenProvider = tokenProvider;
                Configuration = configuration;
            }

            public void AuthorizeRequest(Action<string, string> authTokenSetter)
            {
                tokenProvider.Renew(this);
                authTokenSetter(AuthResult.AccessTokenType, AuthResult.AccessToken);
            }

            public string AccessToken { get { return AuthResult.AccessToken; } }
            public string UserId { get { return AuthResult.UserInfo.UserId; } }

            public LoginType LoginType
            {
                get
                {
                    if (AuthResult.UserInfo.IdentityProvider != null)
                    {
                        return LoginType.LiveId;
                    }
                    return LoginType.OrgId;
                }
            }
        }


        private void ClearCookies()
        {
            NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
        }

        private static class NativeMethods
        {
            internal const int INTERNET_OPTION_END_BROWSER_SESSION = 42;

            [DllImport("wininet.dll", SetLastError = true)]
            internal static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer,
                int lpdwBufferLength);
        }
    }
}