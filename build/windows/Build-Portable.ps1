if (Test-Path .\bin\KeyboardSwitch)
{
    Get-ChildItem -Path .\bin\KeyboardSwitch\* -File -Recurse | foreach { $_.Delete() }
    Start-Sleep -Seconds 0.2
    Remove-Item -Path .\bin\KeyboardSwitch\* -Recurse -Force
}

dotnet publish .\KeyboardSwitch --configuration Release --runtime win10-x64 --framework net6.0-windows `
--self-contained true --output .\bin\KeyboardSwitch --nologo -p:Platform=x64 -p:ContinuousIntegrationBuild=true

dotnet publish .\KeyboardSwitch.Settings --configuration Release --runtime win10-x64 --framework net6.0-windows `
--self-contained true --output .\bin\KeyboardSwitch --nologo -p:Platform=x64 -p:ContinuousIntegrationBuild=true

Remove-Item -Path .\bin\KeyboardSwitch\* -Include *.pdb, *.xml
Remove-Item -Path .\bin\KeyboardSwitch\appsettings.macos.json
Remove-Item -Path .\bin\KeyboardSwitch\appsettings.linux.json

if (Test-Path .\bin\KeyboardSwitch-4.0-x64-win.zip)
{
    Remove-Item -Path .\bin\KeyboardSwitch-4.0-x64-win.zip
}

Compress-Archive -Path .\bin\KeyboardSwitch -DestinationPath .\bin\KeyboardSwitch-4.0-x64-win.zip

Get-ChildItem -Path .\bin\KeyboardSwitch\* -File -Recurse | foreach { $_.Delete() }
Start-Sleep -Seconds 0.2
Remove-Item -Path .\bin\KeyboardSwitch\* -Recurse -Force
Remove-Item -Path .\bin\KeyboardSwitch
