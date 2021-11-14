#!/bin/bash

rm -rf ./bin/KeyboardSwitch 2> /dev/null
mkdir -p ./bin
cd ./bin

dotnet publish ../KeyboardSwitch --configuration Release --runtime linux-x64 --framework net6.0 \
--self-contained true --output ./KeyboardSwitch --nologo -p:Platform=x64

dotnet publish ../KeyboardSwitch.Settings --configuration Release --runtime linux-x64 --framework net6.0 \
--self-contained true --output ./KeyboardSwitch --nologo -p:Platform=x64

find ./KeyboardSwitch -name "*.pdb" -type f -delete
find ./KeyboardSwitch -name "*.xml" -type f -delete
rm ./KeyboardSwitch/appsettings.windows.json
rm ./KeyboardSwitch/appsettings.macos.json

cp ../build/tar/install.sh ./KeyboardSwitch
cp ../build/tar/uninstall.sh ./KeyboardSwitch

rm ./keyboard-switch.tar.gz 2> /dev/null

tar -czf ./keyboard-switch.tar.gz ./KeyboardSwitch

rm -rf ./keyboard-switch 2> /dev/null
