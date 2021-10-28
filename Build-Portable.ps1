if (Test-Path .\bin\KeyboardSwitch) 
{
    Get-ChildItem -Path .\bin\KeyboardSwitch\* -File -Recurse | foreach { $_.Delete() }
    Start-Sleep -Seconds 0.2
    Remove-Item -Path .\bin\KeyboardSwitch\* -Recurse -Force
}

dotnet publish .\KeyboardSwitch --configuration Release --runtime win10-x64 --framework net5.0-windows `
--self-contained true --output .\bin\KeyboardSwitch --nologo -p:Platform=x64 -p:PublishTrimmed=true

dotnet publish .\KeyboardSwitch.Settings --configuration Release --runtime win10-x64 --framework net5.0-windows `
--self-contained true --output .\bin\KeyboardSwitch --nologo -p:Platform=x64 -p:PublishTrimmed=true

Remove-Item -Path .\bin\KeyboardSwitch\* -Include *.pdb, *.xml
Remove-Item -Path .\bin\KeyboardSwitch\appsettings.macos.json
Remove-Item -Path .\bin\KeyboardSwitch\appsettings.linux.json

if (Test-Path .\bin\KeyboardSwitch-Portable.zip) 
{
    Remove-Item -Path .\bin\KeyboardSwitch-Portable.zip
}

Compress-Archive -Path .\bin\KeyboardSwitch -DestinationPath .\bin\KeyboardSwitch-Portable.zip

Get-ChildItem -Path .\bin\KeyboardSwitch\* -File -Recurse | foreach { $_.Delete() }
Start-Sleep -Seconds 0.2
Remove-Item -Path .\bin\KeyboardSwitch\* -Recurse -Force
Remove-Item -Path .\bin\KeyboardSwitch
