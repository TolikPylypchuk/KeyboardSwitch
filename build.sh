#!/bin/bash

rm -rf ./bin/KeyboardSwitch 2> /dev/null
mkdir -p ./bin
cd ./bin

dotnet publish ../KeyboardSwitch --configuration Release --runtime linux-x64 --framework net5.0 \
--self-contained true --output ./KeyboardSwitch --nologo -p:Platform=x64 -p:PublishTrimmed=true

dotnet publish ../KeyboardSwitch.Settings --configuration Release --runtime linux-x64 --framework net5.0 \
--self-contained true --output ./KeyboardSwitch --nologo -p:Platform=x64 -p:PublishTrimmed=true

find ./KeyboardSwitch -name "*.pdb" -type f -delete
find ./KeyboardSwitch -name "*.xml" -type f -delete
find ./KeyboardSwitch -name "*.dylib" -type f -delete
rm ./KeyboardSwitch/appsettings.windows.json
rm ./KeyboardSwitch/appsettings.macos.json
rm ./KeyboardSwitch/uiohook.dll

rm ./KeyboardSwitch.tar.gz 2> /dev/null

tar -czf ./KeyboardSwitch.tar.gz ./KeyboardSwitch

rm -rf ./KeyboardSwitch 2> /dev/null
