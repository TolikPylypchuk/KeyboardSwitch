param([string]$Platform = "x64")

if (Test-Path .\bin\KeyboardSwitch)
{
    Get-ChildItem -Path .\bin\KeyboardSwitch\* -File -Recurse | ForEach-Object { $_.Delete() }
    Start-Sleep -Seconds 0.2
    Remove-Item -Path .\bin\KeyboardSwitch\* -Recurse -Force
}

$Platform = $Platform.ToLower()

$runtime = $Platform -eq "arm64" ? "win10-arm64" : "win10-x64"
$msbuildPlatform = $Platform -eq "arm64" ? "ARM64" : "x64"
$arch = $Platform -eq "arm64" ? "arm64" : "x64"

dotnet publish .\KeyboardSwitch --configuration Release --runtime $runtime --framework net6.0-windows `
--self-contained true --output .\bin\KeyboardSwitch --nologo -p:Platform=$msbuildPlatform `
-p:ContinuousIntegrationBuild=true

dotnet publish .\KeyboardSwitch.Settings --configuration Release --runtime $runtime --framework net6.0-windows `
--self-contained true --output .\bin\KeyboardSwitch --nologo -p:Platform=$msbuildPlatform `
-p:ContinuousIntegrationBuild=true

Remove-Item -Path .\bin\KeyboardSwitch\* -Include *.pdb, *.xml
Remove-Item -Path .\bin\KeyboardSwitch\appsettings.macos.json
Remove-Item -Path .\bin\KeyboardSwitch\appsettings.linux.json
Remove-Item -Path .\bin\KeyboardSwitch\icon.icns
Remove-Item -Path .\bin\KeyboardSwitch\icon.png

Rename-Item -Path .\bin\KeyboardSwitch\appsettings.windows.json -NewName appsettings.json

if (Test-Path .\bin\KeyboardSwitch-4.1-$arch-win.zip)
{
    Remove-Item -Path .\bin\KeyboardSwitch-4.1-$arch-win.zip
}

Compress-Archive -Path .\bin\KeyboardSwitch -DestinationPath .\bin\KeyboardSwitch-4.1-$arch-win.zip

Get-ChildItem -Path .\bin\KeyboardSwitch\* -File -Recurse | ForEach-Object { $_.Delete() }
Start-Sleep -Seconds 0.2
Remove-Item -Path .\bin\KeyboardSwitch\* -Recurse -Force
Remove-Item -Path .\bin\KeyboardSwitch
