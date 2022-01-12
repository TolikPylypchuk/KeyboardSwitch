#!/bin/bash

rm -rf ./bin/keyboard-switch 2> /dev/null
mkdir -p ./bin
cd ./bin

dotnet publish ../KeyboardSwitch --configuration Release --runtime linux-x64 --framework net6.0 \
--self-contained true --output ./keyboard-switch --nologo -p:Platform=x64 -p:ContinuousIntegrationBuild=true

dotnet publish ../KeyboardSwitch.Settings --configuration Release --runtime linux-x64 --framework net6.0 \
--self-contained true --output ./keyboard-switch --nologo -p:Platform=x64 -p:ContinuousIntegrationBuild=true

find ./keyboard-switch -name "*.pdb" -type f -delete
find ./keyboard-switch -name "*.xml" -type f -delete
rm ./keyboard-switch/appsettings.windows.json
rm ./keyboard-switch/appsettings.macos.json

cp ../build/tar/install.sh ./keyboard-switch
cp ../build/tar/uninstall.sh ./keyboard-switch

rm ./keyboard-switch-4.1-x64.tar.gz 2> /dev/null

tar -czf ./keyboard-switch-4.1-x64.tar.gz ./keyboard-switch

rm -rf ./keyboard-switch 2> /dev/null
