namespace Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using IdentityModel.Clients.ActiveDirectory;

    internal class CacheKeyStringifier
    {
        private const char escapeChar = '`';
        private const string escapeString = "`";
        private const char separatorChar = ';';
        private const string separatorString = ";";

        public string ToString(TokenCacheKey key)
        {
            var fields = new string[]
            {
                key.Authority,
                key.ClientId,
                key.ExpiresOn.ToString("o", CultureInfo.InvariantCulture),
                key.FamilyName,
                key.GivenName,
                key.IdentityProviderName,
                key.IsMultipleResourceRefreshToken.ToString(),
                key.IsUserIdDisplayable.ToString(),
                key.Resource,
                key.TenantId,
                key.UserId
            };

            // Escape our separator characters. Using ` instead
            // of \ because hey, powershell.
            for (int i = 0; i < fields.Length; ++i)
            {
                if (fields[i] == null)
                {
                    fields[i] = String.Empty;
                }
                fields[i] = fields[i].Replace(escapeString, escapeString + escapeString);
                fields[i] = fields[i].Replace(separatorString, escapeString + separatorString);
            }
            return String.Join(separatorString, fields);
        }

        private int state;
        private char[] currentToken;
        private char[] input;
        private List<string> fields;
        private int outputIndex;
        private int inputIndex;

        private bool AtEnd { get { return inputIndex == input.Length; } }
        private char CurrentChar { get { return input[inputIndex]; } }
        private void ConsumeInput() 
        {
            ++inputIndex;
        }
        private void OutputChar(char c)
        {
            currentToken[outputIndex++] = c;
        }

        private void OutputField()
        {
            var fieldVal = new string(currentToken, 0, outputIndex);
            fields.Add(fieldVal);
            outputIndex = 0;
        }

        public TokenCacheKey FromString(string key)
        {
            currentToken = new char[key.Length];
            input = key.ToCharArray();
            fields = new List<string>();
            outputIndex = 0;
            inputIndex = 0;
            ProcessString();
            return new TokenCacheKey
            {
                Authority = fields[0],
                ClientId = fields[1],
                ExpiresOn =
                    DateTimeOffset.Parse(fields[2], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                FamilyName = fields[3],
                GivenName = fields[4],
                IdentityProviderName = fields[5],
                IsMultipleResourceRefreshToken = Boolean.Parse(fields[6]),
                IsUserIdDisplayable = Boolean.Parse(fields[7]),
                Resource = fields[8],
                TenantId = fields[9],
                UserId = fields[10]
            };
        }

        // Basic state machine lexer to parse out the fields and
        // properly unescape characters.
        private void ProcessString()
        {
            while (state != 3)
            {
                switch (state)
                {
                    case 0:
                        NextChar();
                        break;
                    case 1:
                        ProcessEscapedChar();
                        break;
                }
            }
        }

        private void NextChar()
        {
            if (AtEnd)
            {
                OutputField();
                state = 3;
            }
            else if (CurrentChar == escapeChar)
            {
                state = 1;
                ConsumeInput();
            }
            else if (CurrentChar == separatorChar)
            {
                OutputField();
                ConsumeInput();
            }
            else
            {
                OutputChar(CurrentChar);
                ConsumeInput();
            }
        }

        private void ProcessEscapedChar()
        {
            if (AtEnd)
            {
                OutputChar(escapeChar);
                OutputField();
                state = 3;
            }
            else if (CurrentChar == escapeChar)
            {
                OutputChar(escapeChar);
                ConsumeInput();
                state = 0;
            }
            else if (CurrentChar == separatorChar)
            {
                OutputChar(separatorChar);
                ConsumeInput();
                state = 0;
            }
            else
            {
                OutputChar(escapeChar);
                OutputChar(CurrentChar);
                state = 0;
            }
        }

    }
}