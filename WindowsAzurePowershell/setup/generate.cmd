@echo off

set output=..\..\Package\Release
set resources=%output%\Resources

echo Removing Resources folder %resources%
rmdir /S /Q %resources%

echo Removing generated NuGet files from %output%
rmdir /S /Q %output%\de
rmdir /S /Q %output%\es
rmdir /S /Q %output%\fr
rmdir /S /Q %output%\it
rmdir /S /Q %output%\ja
rmdir /S /Q %output%\ko
rmdir /S /Q %output%\ru
rmdir /S /Q %output%\zh-Hans
rmdir /S /Q %output%\zh-Hant

echo Delete XML help files for helper dlls from %output%
del %output%\Microsoft.Data.Edm.xml
del %output%\Microsoft.Data.OData.xml
del %output%\Microsoft.Data.Services.Client.xml
del %output%\Microsoft.WindowsAzure.Storage.xml
del %output%\Newtonsoft.Json.xml
del %output%\System.Net.Http.Formatting.xml
del %output%\System.Net.Http.xml
del %output%\System.Spatial.xml
del %output%\System.Net.Http.WebRequest.xml
del %output%\AutoMapper.xml
del %output%\Microsoft.IdentityModel.Clients.ActiveDirectory.xml
del %output%\Microsoft.Threading.Tasks.Extensions.Desktop.xml
del %output%\Microsoft.Threading.Tasks.Extensions.xml
del %output%\Microsoft.Threading.Tasks.xml
del %output%\Moq.xml
del %output%\System.IO.xml
del %output%\System.Net.Http.Extensions.xml
del %output%\System.Net.Http.Primitives.xml

echo Delete config files for dlls from %output%
del %output%\*.dll.config

heat dir %output% -srd -gg -g1 -cg azurecmdfiles -sfrag -dr PowerShellFolder -var var.sourceDir -o azurecmdfiles.wxi