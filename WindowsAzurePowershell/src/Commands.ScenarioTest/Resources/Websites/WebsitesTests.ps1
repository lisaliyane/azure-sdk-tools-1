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

########################################################################### General Websites Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests any cloud based cmdlet with invalid credentials and expect it'll throw an exception.
#>
function Test-WithInvalidCredentials
{
	param([ScriptBlock] $cloudCmdlet)
	
	# Setup
	Remove-AllSubscriptions

	# Test
	Assert-Throws $cloudCmdlet "Call Set-AzureSubscription and Select-AzureSubscription first."
}

########################################################################### Remove-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Remove-AzureWebsite with existing name
#>
function Test-RemoveAzureServiceWithValidName
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	$expected = "The website $name was not found. Please specify a valid website name."

	# Test
	Remove-AzureWebsite $name -Force

	# Assert
	Assert-Throws { Get-AzureWebsite $name } $expected
}

<#
.SYNOPSIS
Tests Remove-AzureWebsite with non existing name
#>
function Test-RemoveAzureServiceWithNonExistingName
{
	Assert-Throws { Remove-AzureWebsite "OneSDKNotExisting" -Force } "The website OneSDKNotExisting was not found. Please specify a valid website name."
}

<#
.SYNOPSIS
Tests Remove-AzureWebsite with WhatIf
#>
function Test-RemoveAzureServiceWithWhatIf
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	$expected = "The website $name was not found. Please specify a valid website name."

	# Test
	Remove-AzureWebsite $name -Force -WhatIf
	Remove-AzureWebsite $name -Force

	# Assert
	Assert-Throws { Get-AzureWebsite $name } $expected
}

########################################################################### Get-AzureWebsiteLog Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Get-AzureWebsiteLog with -Tail
#>
function Test-GetAzureWebsiteLogTail
{
	# Setup
	New-BasicLogWebsite
	$website = $global:currentWebsite
	$client = New-Object System.Net.WebClient
	$uri = "http://" + $website.HostNames[0]
	$client.BaseAddress = $uri
	$count = 0
	cd ..

	#Test
	Get-AzureWebsiteLog -Name $website.Name -Tail -Message "㯑䲘䄂㮉" | % {
		if ($_ -like "*㯑䲘䄂㮉*") { exit; }
		Retry-DownloadString $client $uri
		$count++
		if ($count -gt 50) { throw "Logs were not found"; }
	}
}

<#
.SYNOPSIS
Tests Get-AzureWebsiteLog with -Tail with special characters in uri.
#>
function Test-GetAzureWebsiteLogTailUriEncoding
{
	# Setup
	New-BasicLogWebsite
	$website = $global:currentWebsite
	$client = New-Object System.Net.WebClient
	$uri = "http://" + $website.HostNames[0]
	$client.BaseAddress = $uri
	$count = 0
	cd ..

	#Test
	Get-AzureWebsiteLog -Name $website.Name -Tail -Message "mes/a:q;" | % {
		if ($_ -like "*mes/a:q;*") { exit; }
		Retry-DownloadString $client $uri
		$count++
		if ($count -gt 50) { throw "Logs were not found"; }
	}
}

<#
.SYNOPSIS
Tests Get-AzureWebsiteLog with -Tail
#>
function Test-GetAzureWebsiteLogTailPath
{
	# Setup
	New-BasicLogWebsite
	$website = $global:currentWebsite
	$client = New-Object System.Net.WebClient
	$uri = "http://" + $website.HostNames[0]
	$client.BaseAddress = $uri
	Set-AzureWebsite -RequestTracingEnabled $true -HttpLoggingEnabled $true -DetailedErrorLoggingEnabled $true
	1..10 | % { Retry-DownloadString $client $uri }
	Start-Sleep -Seconds 30
	cd ..

	#Test
	$retry = $false
	do
	{
		try
		{
			Get-AzureWebsiteLog -Name $website.Name -Tail -Path http | % {
				if ($_ -like "*")
				{
					exit
				}
				throw "HTTP path is not reached"
			}
		}
		catch
		{
			if ($_.Exception.Message -eq "One or more errors occurred.")
			{
				$retry = $true;
				Write-Warning "Retry Test-GetAzureWebsiteLogTailPath"
				continue;
			}

			throw $_.Exception
		}
	} while ($retry)
}

<#
.SYNOPSIS
Tests Get-AzureWebsiteLog with -ListPath
#>
function Test-GetAzureWebsiteLogListPath
{
	# Setup
	New-BasicLogWebsite

	#Test
	$retry = $false
	do
	{
		try
		{
			$actual = Get-AzureWebsiteLog -ListPath;
			$retry = $false
		}
		catch
		{
			if ($_.Exception.Message -like "For security reasons DTD is prohibited in this XML document.*")
			{
				$retry = $true;
				Write-Warning "Retry Test-GetAzureWebsiteLogListPath"
				continue;
			}
			cd ..
			throw $_.Exception
		}
	} while ($retry)

	# Assert
	Assert-AreEqual 1 $actual.Count
	Assert-AreEqual "Git" $actual
	cd ..
}

########################################################################### Get-AzureWebsite Scenario Tests ###########################################################################
<#
.SYNOPSIS
Test Kudu apps
#>
function Test-KuduAppsExpressApp
{
	$GIT_USERNAME = $env:GIT_USERNAME
	
	# Setup
	$siteName = Get-WebsiteName
	Mkdir $siteName
	cd $siteName
	
	# Test
	$command = "install -g express";
	Start-Process npm $command -WAIT

	express
	$webSite = New-AzureWebSite $siteName -Git –PublishingUsername $GIT_USERNAME
	
	# Assert
	Assert-NotNull { $webSite } "Site $siteName created failed"
	Assert-Exists "..\$siteName\iisnode.yml"
	
	# CleanUp
	Remove-AzureWebsite -Name $siteName –Force
}

<#
.SYNOPSIS
Tests Get-AzureWebsite
#>
function Test-GetAzureWebsite
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name

	#Test
	$config = Get-AzureWebsite -Name $name

	# Assert
	Assert-AreEqual $name $config.Name
}

<#
.SYNOPSIS
Tests GetAzureWebsite with a stopped site and expects to proceed.
#>
function Test-GetAzureWebsiteWithStoppedSite
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	Stop-AzureWebsite $name

	#Test
	$website = Get-AzureWebsite $name

	# Assert
	Assert-NotNull { $website }
}

########################################################################### Start-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Start-AzureWebsite happy path.
#>
function Test-StartAzureWebsite
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	Stop-AzureWebsite $name

	# Test
	Start-AzureWebsite $name

	# Assert
	$website = Get-AzureWebsite $name
	Assert-AreEqual "Running" $website.State
}

########################################################################### Stop-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Stop-AzureWebsite happy path.
#>
function Test-StopAzureWebsite
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name

	# Test
	Stop-AzureWebsite $name

	# Assert
	$website = Get-AzureWebsite $name
	Assert-AreEqual $name $website.Name
}

########################################################################### Restart-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Restart-AzureWebsite happy path.
#>
function Test-RestartAzureWebsite
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name

	# Test
	Restart-AzureWebsite $name

	# Assert
	$website = Get-AzureWebsite $name
	Assert-AreEqual "Running" $website.State
}

########################################################################### Enable-AzureWebsiteApplicationDiagnostic Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Enable-AzureWebsiteApplicationDiagnostic with storage table
#>
function Test-EnableApplicationDiagnosticOnTableStorage
{
	# Setup
	$name = Get-WebsiteName
	$storageName = $(Get-WebsiteName).ToLower()
	$locations = Get-AzureLocation
	$defaultLocation = $locations[0].Name
	New-AzureWebsite $name
	New-AzureStorageAccount -ServiceName $storageName -Location $defaultLocation
	
	# Test
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Warning -StorageAccountName $storageName

	# Assert
	$website = Get-AzureWebsite $name
	Assert-True { $website.AzureTableTraceEnabled }
	Assert-AreEqual Warning $website.AzureTableTraceLevel
	Assert-NotNull { $website.ConnectionStrings | ?{ $_.Name -eq "CLOUD_STORAGE_ACCOUNT" } }

	# Cleanup
	Remove-AzureStorageAccount $storageName
}

<#
.SYNOPSIS
Tests Enable-AzureWebsiteApplicationDiagnostic with file system
#>
function Test-EnableApplicationDiagnosticOnFileSystem
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name

	# Test
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -File -LogLevel Warning

	# Assert
	$website = Get-AzureWebsite $name
	Assert-True { $website.AzureDriveTraceEnabled }
	Assert-AreEqual Warning $website.AzureDriveTraceLevel
}

<#
.SYNOPSIS
Tests Enable-AzureWebsiteApplicationDiagnostic when updating a log level and expects to pass.
#>
function Test-UpdateTheDiagnositicLogLevel
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -File -LogLevel Verbose

	# Test
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -File -LogLevel Warning

	# Assert
	$website = Get-AzureWebsite $name
	Assert-True { $website.AzureDriveTraceEnabled }
	Assert-AreEqual Warning $website.AzureDriveTraceLevel
}

<#
.SYNOPSIS
Tests reconfiguring the table storage diagnostic settings information.
#>
function Test-ReconfigureStorageAppDiagnostics
{
	# Setup
	$name = Get-WebsiteName
	$storageName = $(Get-WebsiteName).ToLower()
	$newStorageName = $(Get-WebsiteName).ToLower()
	$locations = Get-AzureLocation
	$defaultLocation = $locations[0].Name
	New-AzureWebsite $name
	New-AzureStorageAccount -ServiceName $storageName -Location $defaultLocation
	New-AzureStorageAccount -ServiceName $newStorageName -Location $defaultLocation
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Warning -StorageAccountName $storageName

	# Test
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Verbose -StorageAccountName $newStorageName

	# Assert
	$website = Get-AzureWebsite $name
	Assert-True { $website.AzureTableTraceEnabled }
	Assert-AreEqual Verbose $website.AzureTableTraceLevel
	Assert-True { ($website.ConnectionStrings | ?{ $_.Name -eq "CLOUD_STORAGE_ACCOUNT" }).ConnectionString -like "*" + $newStorageName + "*" }

	# Cleanup
	Remove-AzureStorageAccount $storageName
}

<#
.SYNOPSIS
Tests Enable-AzureWebsiteApplicationDiagnostic with not existing storage service.
#>
function Test-ThrowsForInvalidStorageAccountName
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	
	# Test
	Assert-Throws { Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Warning -StorageAccountName "notexsiting" }
}

########################################################################### Disable-AzureWebsiteApplicationDiagnostic Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Disable-AzureWebsiteApplicationDiagnostic with storage table
#>
function Test-DisableApplicationDiagnosticOnTableStorage
{
	# Setup
	$name = Get-WebsiteName
	$storageName = $(Get-WebsiteName).ToLower()
	$locations = Get-AzureLocation
	$defaultLocation = $locations[0].Name
	New-AzureWebsite $name
	New-AzureStorageAccount -ServiceName $storageName -Location $defaultLocation
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Warning -StorageAccountName $storageName
	
	# Test
	Disable-AzureWebsiteApplicationDiagnostic -Name $name -Storage

	# Assert
	$website = Get-AzureWebsite $name
	Assert-False { $website.AzureTableTraceEnabled }
	Assert-AreEqual Warning $website.AzureTableTraceLevel
	Assert-NotNull { $website.ConnectionStrings | ?{ $_.Name -eq "CLOUD_STORAGE_ACCOUNT" } }

	# Cleanup
	Remove-AzureStorageAccount $storageName
}

<#
.SYNOPSIS
Tests Disable-AzureWebsiteApplicationDiagnostic with file system
#>
function Test-DisableApplicationDiagnosticOnFileSystem
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -File -LogLevel Warning

	# Test
	Disable-AzureWebsiteApplicationDiagnostic -Name $name -File

	# Assert
	$website = Get-AzureWebsite $name
	Assert-False { $website.AzureDriveTraceEnabled }
	Assert-AreEqual Warning $website.AzureDriveTraceLevel
}

<#
.SYNOPSIS
Tests Disable-AzureWebsiteApplicationDiagnostic with storage and file
#>
function Test-DisableApplicationDiagnosticOnTableStorageAndFile
{
	# Setup
	$name = Get-WebsiteName
	$storageName = $(Get-WebsiteName).ToLower()
	$locations = Get-AzureLocation
	$defaultLocation = $locations[0].Name
	New-AzureWebsite $name
	New-AzureStorageAccount -ServiceName $storageName -Location $defaultLocation
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Warning -StorageAccountName $storageName
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -File -LogLevel Warning
	
	# Test
	Disable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -File

	# Assert
	$website = Get-AzureWebsite $name
	Assert-False { $website.AzureTableTraceEnabled }
	Assert-False { $website.AzureDriveTraceEnabled }
	Assert-NotNull { $website.ConnectionStrings | ?{ $_.Name -eq "CLOUD_STORAGE_ACCOUNT" } }

	# Cleanup
	Remove-AzureStorageAccount $storageName
}

<#
.SYNOPSIS
Tests Disable-AzureWebsiteApplicationDiagnostic with file. Makes sure it disables file only.
#>
function Test-DisablesFileOnly
{
	# Setup
	$name = Get-WebsiteName
	$storageName = $(Get-WebsiteName).ToLower()
	$locations = Get-AzureLocation
	$defaultLocation = $locations[0].Name
	New-AzureWebsite $name
	New-AzureStorageAccount -ServiceName $storageName -Location $defaultLocation
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Warning -StorageAccountName $storageName
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -File -LogLevel Verbose
	
	# Test
	Disable-AzureWebsiteApplicationDiagnostic -Name $name -File

	# Assert
	$website = Get-AzureWebsite $name
	Assert-True { $website.AzureTableTraceEnabled }
	Assert-False { $website.AzureDriveTraceEnabled }
	Assert-NotNull { $website.ConnectionStrings | ?{ $_.Name -eq "CLOUD_STORAGE_ACCOUNT" } }

	# Cleanup
	Remove-AzureStorageAccount $storageName
}

<#
.SYNOPSIS
Tests Disable-AzureWebsiteApplicationDiagnostic with file. Makes sure it disables storage only.
#>
function Test-DisablesStorageOnly
{
	# Setup
	$name = Get-WebsiteName
	$storageName = $(Get-WebsiteName).ToLower()
	$locations = Get-AzureLocation
	$defaultLocation = $locations[0].Name
	New-AzureWebsite $name
	New-AzureStorageAccount -ServiceName $storageName -Location $defaultLocation
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -File -LogLevel Verbose
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Warning -StorageAccountName $storageName
	
	# Test
	Disable-AzureWebsiteApplicationDiagnostic -Name $name -Storage

	# Assert
	$website = Get-AzureWebsite $name
	Assert-True { $website.AzureDriveTraceEnabled }
	Assert-False { $website.AzureTableTraceEnabled }
	Assert-NotNull { $website.ConnectionStrings | ?{ $_.Name -eq "CLOUD_STORAGE_ACCOUNT" } }

	# Cleanup
	Remove-AzureStorageAccount $storageName
}

<#
.SYNOPSIS
Tests Disable-AzureWebsiteApplicationDiagnostic with file. Makes sure it disables storage and table.
#>
function Test-DisablesBothByDefault
{
	# Setup
	$name = Get-WebsiteName
	$storageName = $(Get-WebsiteName).ToLower()
	$locations = Get-AzureLocation
	$defaultLocation = $locations[0].Name
	New-AzureWebsite $name
	New-AzureStorageAccount -ServiceName $storageName -Location $defaultLocation
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -Storage -LogLevel Warning -StorageAccountName $storageName
	Enable-AzureWebsiteApplicationDiagnostic -Name $name -File -LogLevel Verbose
	
	# Test
	Disable-AzureWebsiteApplicationDiagnostic -Name $name

	# Assert
	$website = Get-AzureWebsite $name
	Assert-False { $website.AzureTableTraceEnabled }
	Assert-False { $website.AzureDriveTraceEnabled }
	Assert-NotNull { $website.ConnectionStrings | ?{ $_.Name -eq "CLOUD_STORAGE_ACCOUNT" } }

	# Cleanup
	Remove-AzureStorageAccount $storageName
}

########################################################################### Get-AzureWebsiteLocation Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Get-AzureWebsiteLocation and expects to return valid websites.
#>
function Test-GetAzureWebsiteLocation
{
	# Test
	$locations = Get-AzureWebsiteLocation;

	# Assert
	Assert-NotNull { $locations }
	Assert-True { $locations.Count -gt 0 }
}

<#
.SYNOPSIS
Test Get-AzureWebsite list none
#>
function Test-GetAzureWebSiteListNone
{
	 Get-AzureWebsite | Remove-AzureWebsite –Force
	 Assert-True { (Get-AzureWebsite) -eq $null}
}

<#
.SYNOPSIS
Tests Get-AzureWebsite list all
#>
function Test-AzureWebSiteListAll
{
	#Setup
	$name1 = Get-WebsiteName
	$name2 = Get-WebsiteName
	$name3 = Get-WebsiteName

	#Test
	New-AzureWebsite $name1
	New-AzureWebsite $name2
	New-AzureWebsite $name3

	$name = (Get-AzureWebsite).Name
	Assert-True {$name.Contains($name1)}
	Assert-True {$name.Contains($name2)}
	Assert-True {$name.Contains($name3)}

	# Cleanup
	Remove-AzureWebsite $name1 -Force
	Remove-AzureWebsite $name2 -Force
	Remove-AzureWebsite $name3 -Force
}

<#
.SYNOPSIS
Test Get-AzureWebsite show single site
#>
function Test-AzureWebSiteShowSingleSite
{
	# Setup
	$name1 = Get-WebsiteName
	$name2 = Get-WebsiteName
	$name3 = Get-WebsiteName

	#Test
	New-AzureWebsite $name1
	New-AzureWebsite $name2
	New-AzureWebsite $name3
	Assert-True { (Get-AzureWebsite $name1).Name -eq  $name1 }	
	Assert-True { (Get-AzureWebsite $name2).Name -eq  $name2 }	
	Assert-True { (Get-AzureWebsite $name3).Name -eq  $name3 }	
	
	# Cleanup
	Remove-AzureWebsite $name1 -Force
	Remove-AzureWebsite $name2 -Force
	Remove-AzureWebsite $name3 -Force
} 

########################################################################### Azurewebsite Git Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests New azure web site with git hub.
#>
function Test-NewAzureWebSiteMultipleCreds
{
	$GIT_USERNAME = $env:GIT_USERNAME
	$GIT_PASSWORD = $env:GIT_PASSWORD

	# Setup
	$siteName = Get-WebsiteName
	Set-Location "\"
	mkdir $siteName
	Set-Location $siteName
	
	# Test
	New-AzureWebsite $siteName -Git -PublishingUsername $GIT_USERNAME
	$webSite = Get-AzureWebsite -Name $siteName
	
	# Verify publishingusername & publishingpassword in git remote
	$webSite = Get-AzureWebsite -Name $siteName
	$gitRemoteList = git remote -v
	$expectedRemoteUri = "https://" + $GIT_USERNAME + "@" + $webSite.EnabledHostNames[1] + "/" + $webSite.Name + ".git"
	Assert-True { $gitRemoteList[0].Contains($expectedRemoteUri)}

	# Install express
	Npm-InstallExpress

	# Push local git to website
	Git-PushLocalGitToWebSite $siteName
	
	# Verify browse website
	$siteStatusRunning = Retry-Function { return (Get-AzureWebsite -Name $siteName).State -eq "Running" } $null 4 1
	$deploymentStatusSuccess = Retry-Function { return (Get-AzureWebSiteDeployment $siteName).Status.ToString() -eq "Success" } $null 8 2
	if (($siteStatusRunning -eq $true) -and ($deploymentStatusSuccess -eq $true))
	{
		$url = "http://" + $webSite.EnabledHostNames[0]
		$expectedString = "Welcome to Express"
		Assert-True { Test-ValidateResultInBrowser ($url) $expectedString }
	}
	else
	{
		throw "Web site or git repository is not ready for browse"
	}
	
	# CleanUP
	if($webSite -ne $null)
	{
		Remove-AzureWebsite $siteName -Force
	}   
}

<#
.SYNOPSIS
Tests New azure web site with git hub.
#>
function Test-NewAzureWebSiteGitHubAllParms
{
	$GitHub_USERNAME = $env:GITHub_USERNAME
	$GitHub_PASSWORD = $env:GITHub_PASSWORD
	$GitHub_REPO = $env:GITHub_USERNAME + "/WebChatDefault-0802"
	
	# Setup
	$siteName = Get-WebsiteName
	Set-Location "\"
	mkdir $siteName
	Set-Location $siteName

	# Test
	$myCreds = New-Object "System.Management.Automation.PSCredential" ($GitHub_USERNAME, (ConvertTo-SecureString $GitHub_PASSWORD -AsPlainText -Force))
	$webSite = New-AzureWebsite $siteName -Location (Get-AzureWebsiteLocation)[0] -GitHub -GithubRepository $GitHub_REPO -GithubCredentials $myCreds

	$siteStatusRunning = Retry-Function { (Get-AzureWebsite -Name $siteName).State -eq "Running" } $null 4 2
	$deploymentStatusSuccess = Retry-Function { (Get-AzureWebSiteDeployment $siteName).Status.ToString() -eq "Success" } $null 8 3
	if (($siteStatusRunning -eq $true) -and ($deploymentStatusSuccess -eq $true))
	{
		Assert-True { Test-ValidateResultInBrowser ("http://" + $WebSite.HostNames[0]) "0.8.2" }
	}
	else
	{
		throw "Web site or git repository is not ready for browse"
	}
	
	# Cleanup
	if($webSite -ne $null)
	{
		Remove-AzureWebsite $siteName -Force
	}
}

<#
.SYNOPSIS
Test New azure web site then update git deployment
#>
function Test-NewAzureWebSiteUpdateGit
{
	$GIT_USERNAME = $env:GIT_USERNAME
	$GIT_PASSWORD = $env:GIT_PASSWORD

	# Setup
	$siteName = Get-WebsiteName
	Set-Location "\"
	mkdir $siteName
	Set-Location $siteName

	# Test
	New-AzureWebSite $siteName
	# Set the ErrorActionPreference as "SilentlyContinue" to work around "The website already exist" exception
	$oldErrorActionPreferenceValue = $ErrorActionPreference
	$ErrorActionPreference = "SilentlyContinue"
	New-AzureWebSite $siteName -Git -Publishingusername:$GIT_USERNAME
	$ErrorActionPreference = $oldErrorActionPreferenceValue

	# Verify publishingusername & publishingpassword in git remote
	$webSite = Get-AzureWebsite -Name $siteName
	$gitRemoteList = git remote -v
	$expectedRemoteUri = "https://" + $GIT_USERNAME + "@" + $webSite.EnabledHostNames[1] + "/" + $webSite.Name + ".git"
	Assert-True { $gitRemoteList[0].Contains($expectedRemoteUri)}

	# Install express
	Npm-InstallExpress

	# Push local git to website
	Git-PushLocalGitToWebSite $siteName

	# Verify browse website
	$siteStatusRunning = Retry-Function { return (Get-AzureWebsite -Name $siteName).State -eq "Running" } $null 4 1
	$deploymentStatusSuccess = Retry-Function { return (Get-AzureWebSiteDeployment $siteName).Status.ToString() -eq "Success" } $null 8 2
	if (($siteStatusRunning -eq $true) -and ($deploymentStatusSuccess -eq $true))
	{
		$url = "http://" + $webSite.EnabledHostNames[0]
		$expectedString = "Welcome to Express"
		Assert-True { Test-ValidateResultInBrowser ($url) $expectedString }
	}
	else
	{
		throw "Web site or git repository is not ready for browse"
	}

	# CleanUp
	if($webSite -ne $null)
	{
		Remove-AzureWebsite $siteName -Force
	}
}