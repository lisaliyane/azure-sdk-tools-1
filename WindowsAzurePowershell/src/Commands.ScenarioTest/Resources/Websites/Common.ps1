# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

$createdWebsites = @()
$currentWebsite = $null

<#
.SYNOPSIS
Gets valid website name.
#>
function Get-WebsiteName
{
	return "OneSDKWebsite" + (Get-Random).ToString()
}

<#
.SYNOPSIS
Creates websites with the count specified

.PARAMETER count
The number of websites to create.
#>
function New-Website
{
	param([int] $count)
	
	1..$count | % {
		$name = Get-WebsiteName
		New-AzureWebsite $name
		$global:createdWebsites += $name
	}
}

<#
.SYNOPSIS
Removes all websites in the current subscription.
#>
function Initialize-WebsiteTest
{
	Get-AzureWebsite | Remove-AzureWebsite -Force
}

<#
.SYNOPSIS
Clones git repo
#>
function Clone-GitRepo
{
	param([string] $repo, [string] $dir)

	$cloned = $false
	do
	{
		try
		{
			git clone $repo $dir
			$cloned = $true
		}
		catch
		{
			# Do nothing
		}
	}
	while (!$cloned)
}

<#
.SYNOPSIS
Creates new website using the sample log app template.
#>
function New-BasicLogWebsite
{
	$name = Get-WebsiteName
	Clone-GitRepo https://github.com/wapTestApps/basic-log-app.git $name
	$password = ConvertTo-SecureString $githubPassword -AsPlainText -Force
	$credentials = New-Object System.Management.Automation.PSCredential $githubUsername,$password 
	cd $name
	$global:currentWebsite = New-AzureWebsite -Name $name -Github -GithubCredentials $credentials -GithubRepository wapTestApps/basic-log-app
}

<#
.SYNOPSIS
Retries DownloadString
#>
function Retry-DownloadString
{
	param([object] $client, [string] $uri)

	$retry = $false

	do
	{
		try
		{
			$client.DownloadString($uri)
			$retry = $false
		}
		catch
		{
			$retry = $true
			Write-Warning "Retry calling $client.DownloadString"
		}
	}
	while ($retry)
}

<#
.SYNOPSIS
Get downloadString and verify expected string
#>
function Test-ValidateResultInBrowser
{
       param([string] $uri, [string] $expectedString)
       $client = New-Object System.Net.WebClient
       $resultString = $client.DownloadString($uri)
       return $resultString.ToUpper().Contains($expectedString.ToUpper())
}

<#
.SYNOPSIS
Runs npm and verifies the results.

.PARAMETER command
The npm command to run
#>

function Npm-InstallExpress
{
	try
	{
		$command = "install -g express";
		Start-Process npm $command -WAIT
		"Y" | express
		if([system.IO.File]::Exists("server.js"))
		{
			del server.js
		}
		mv app.js server.js
		npm install 
	}
	catch
	{
		Write-Warning "Expected warning exist when npm install, ignore it"
	}
}

<#
.SYNOPSIS
Push local git repo to website.

.PARAMETER command
Target site name to push
#>

function Git-PushLocalGitToWebSite
{
	param([string] $siteName)
	$webSite = Get-AzureWebsite -Name $siteName

	# Expected warning: LF will be replaced by CRLF in node_modules/.bin/express." when run git command
	Assert-Throws { git add -A } 
	$commitString = "Update azurewebsite with local git"
	Assert-Throws { git commit -m $commitString }

	$remoteAlias = "azureins"
	$remoteUri = "https://" + $env:GIT_USERNAME + ":" + $env:GIT_PASSWORD + "@" + $webSite.EnabledHostNames[1] + "/" + $webSite.Name + ".git"
	git remote add $remoteAlias $remoteUri

	# Disable Git SSL verification for Windows Azure Pack
	git config --local http.sslVerify false
	
	# Expected message "remote: Updating branch 'master'"
	Assert-Throws { git push $remoteAlias master }
}