if (Test-Path .\bin\KeyboardSwitch\) 
{
    Remove-Item -Path .\bin\KeyboardSwitch\ -Recurse
}

dotnet publish .\KeyboardSwitch\ --configuration Release --runtime win10-x64 --framework net5.0-windows `
--self-contained true --output .\bin\KeyboardSwitch --nologo -p:Platform=x64 -p:PublishTrimmed=true

dotnet publish .\KeyboardSwitch.Settings\ --configuration Release --runtime win10-x64 --framework net5.0-windows `
--self-contained true --output .\bin\KeyboardSwitch --nologo -p:Platform=x64 -p:PublishTrimmed=true

Remove-Item -Path .\bin\KeyboardSwitch\ -Include *.pdb, *.xml

if (Test-Path .\bin\KeyboardSwitch-Portable.zip) 
{
    Remove-Item -Path .\bin\KeyboardSwitch-Portable.zip
}

Compress-Archive -Path .\bin\KeyboardSwitch\ -DestinationPath .\bin\KeyboardSwitch-Portable.zip

Remove-Item -Path .\bin\KeyboardSwitch\ -Recurse
