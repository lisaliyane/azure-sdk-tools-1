# Windows Azure PowerShell

This repository contains a set of PowerShell cmdlets for developers and administrators to develop, deploy and manage Windows Azure applications.

* For documentation on how to build and deploy applications to Windows Azure please see the [Windows Azure Developer Center](http://www.windowsazure.com/en-us/develop).
* For comprehensive documentation on the developer cmdlets see [How to use Windows Azure PowerShell](http://www.windowsazure.com/en-us/develop/nodejs/how-to-guides/powershell-cmdlets/).
* For comprehensive documentation on the full set of Windows Azure cmdlets see [Windows Azure Management Center](http://go.microsoft.com/fwlink/?linkID=254459&clcid=0x409).

# Cmdlets Features

* Account
  * Get and import Azure publish settings
* Environment
  * Get the differnet out-of-box Windows Azure environments
  * Add/Set/Remove customized environments (like your Windows Azure Pack environments)
  * Get Azure publish settings for a particular environment
* Subscription
  * Manage Azure subscription
  * Manage AffinityGroup
* Website
  * Manage website, such as CRUD, start and stop.
  * Diagnostics
      * Configure site and application diagnostics
      * Log streaming
      * Save log
* Cloud Services
  * Create scaffolding for cloud service and role. Role support includes Node.js, PHP, Django and Cache.
  * Manage cloud service and role, such as CRUD, start and stop.
  * Enable/Disable remote desktop.
  * Start/Stop Azure emulator.
  * Manage certificate.
  * Manage cloud service extensions
    * Remote desktoo
    * Diagnostics 
* Storage
  * Manage storage account and access key.
  * Manage storage container and blob.
  * Copy storage blob.
  * Manage storage table.
  * Manage storage queue.
* SQL Azure
  * CRUD support for database server, database and firewall rule.
  * Get database server quota.
  * Get/Set database server service objective.
* Service Bus
  * Manage service bus namespaces.
* VM
  * Manage VM, such as CRUD, import/export and start/stop/restart.
  * Manage VM image, such as CRUD.
  * Manage disk, such as CRUD.
  * Manage VM endpoint, such as CRUD and ACL.
  * Get/Set VM sub net.
  * Manage certificate and SSH key.
  * PowerShell remoting
* Deployment
  * Manage deployment, such as CRUD, move, upgrade and restore.
  * Get/Create DNS settings of a deployment.
* VNet
  * Manage virtual network config, connection and gateway.
* Store
  * View available Windows Azure Store Add-On plans.
  * Purchase, view, upgrade and remove Windows Azure Store Add-On instances.
* Utility
  * Test whether a name is avaiable. Currently support cloud service name, storage account name and service bus namespace name.
  * Get the list of geo locations supported by Azure.
  * Get the list of OS supported by Azure.
  * Direct you to Azure portal.

For detail descriptions and examples of the cmdlets, type
* ```help azure``` to get all the cmdlets.
* ```help node-dev``` to get all Node.js development related cmdlets.
* ```help php-dev``` to get all PHP development related cmdlets.
* ```help python-dev``` to get all Python development related cmdlets.
* ```help <cmdlet name>``` to get the details of a specific cmdlet.

# Supported Environments

* [Windows Azure](http://www.windowsazure.com/)
* [Windows Azure Pack](http://www.microsoft.com/en-us/server-cloud/windows-azure-pack.aspx)
* [Windows Azure China](http://www.windowsazure.cn/)

# Getting Started

## Microsoft Web Platform Installer

1. Install [Microsoft Web Platform Installer](http://www.microsoft.com/web/downloads/platform.aspx).
2. Open Microsoft Web Platform Installer and search for __Windows Azure PowerShell__.
3. Install.

You can also find the standalone installers for all the versions at [Downloads](https://github.com/WindowsAzure/azure-sdk-tools/wiki/Downloads)

## Source Code

1. Download the source code from GitHub repo
2. Follow the [Windows Azure PowerShell Developer Guide](https://github.com/WindowsAzure/azure-sdk-tools/wiki/Windows-Azure-PowerShell-Developer-Guide)

## Supported PowerShell Versions

* 0.6.9 or lower
  * [Windows PowerShell 2.0](http://technet.microsoft.com/en-us/scriptcenter/dd742419)
  * [Windows PowerShell 3.0](http://www.microsoft.com/en-us/download/details.aspx?id=34595)
* 0.6.10 to higher
  * [Windows PowerShell 3.0](http://www.microsoft.com/en-us/download/details.aspx?id=34595)

# Quick Start

In general, following are the steps to start using Windows Azure PowerShell

1. Get the publish settings information of your subscription
2. Import the information into Windows Azure PowerShell
3. Use the cmdlets

The first step can be different for different environment you are targeting. Following are detail instructions for each supported environment.

## Windows Azure

```powershell
# Download a file which contains the publish settings information of your subscription.
# This will open a browser window and ask you to log in to get the file.
Get-AzurePublishSettingsFile

# Import the file you just downloaded.
# Notice that the file contains credential of your subscription so you don't want to make it public
# (like check in to source control, etc.).
Import-AzurePublishSettingsFile "<file location>"

# Use the cmdlets to manage your services/applications
New-AzureWebsite -Name mywebsite -Location "West US"
```
## Windows Azure China

```powershell
# Check the environment supported by your Windows Azure PowerShell installation.
Get-AzureEnvironment

# Download a file which contains the publish settings information of your subscription.
# Use the -Environment parameter to target Windows Azure China.
# This will open a browser window and ask you to log in to get the file.
Get-AzurePublishSettingsFile -Environment "AzureChinaCloud"

# Import the file you just downloaded.
# Notice that the file contains credential of your subscription so you don't want to make it public
# (like check in to source control, etc.).
Import-AzurePublishSettingsFile "<file location>"

# Use the cmdlets to manage your services/applications
New-AzureWebsite -Name mywebsite -Location "China East"
```

## Windows Azure Pack

```powershell
# Add your Windows Azure Pack environment to your Windows Azure PowerShell installation.
# You will need to know the following information of your Windows Azure Pack environment.
# 1. URL to download the publish settings file    Mandatory
# 2. Management service endpoint                  Optional
# 3. Management Portal URL                        Optional
# 4. Storage service endpoint                     Optional
Add-WAPackEnvironment -Name "MyWAPackEnv" `
    -PublishSettingsFileUrl "URL to download the publish settings file>" `
    -ServiceEndpoint "<Management service endpoint>" `
    -ManagementPortalUrl "<Storage service endpoint>" `
    -StorageEndpoint "<Management Portal URL>"

# Download a file which contains the publish settings information of your subscription.
# Use the -Environment parameter to target your Windows Azure Pack environment.
# This will open a browser window and ask you to log in to get the file.
Get-WAPackPublishSettingsFile -Environment "MyWAPackEnv"

# Import the file you just downloaded.
# Notice that the file contains credential of your subscription so you don't want to make it public
# (like check in to source control, etc.).
Import-WAPackPublishSettingsFile "<file location>"

# Use the cmdlets to manage your services/applications
New-WAPackWebsite -Name mywebsite
```

# Find Your Way

All the cmdlets can be put into 3 categories:

1. Cmdlets support both Windows Azure and Windows Azure Pack
2. Cmdlets only support both Windows Azure
3. Cmdlets only support Windows Azure Pack

* For category 1, we are using an "Azure" prefix in the cmdlet name and adding an alias with "WAPack" prefix.
* For category 2, we are using an "Azure" prefix in the cmdlet name.
* For category 2, we are using an "WAPack" prefix in the cmdlet name.

So you can use the following cmdlet to find out all the cmdlets for your environment

```powershell
# Return all the cmdlets for Windows Azure
Get-Command *Azure*

# Return all the cmdlets for Windows Azure Pack
Get-Command *WAPack*
```

If you want to migrate some scripts from Windows Azure to Windows Azure Pack or vice versa, as long as the cmdlets you are using are in category 1, you should be able to migrate smoothly.

# Need Help?

Be sure to check out the [Windows Azure Developer Forums on Stack Overflow](http://go.microsoft.com/fwlink/?LinkId=234489) if you have trouble with the provided code.

# Contribute Code or Provide Feedback

If you would like to become an active contributor to this project please follow the instructions provided in [Windows Azure Projects Contribution Guidelines](http://windowsazure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/WindowsAzure/azure-sdk-tools/issues) section of the project.

# Learn More

* [Windows Azure Developer Center](http://www.windowsazure.com/en-us/develop)
