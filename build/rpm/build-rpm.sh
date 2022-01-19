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
rm ./keyboard-switch/libuiohook.so.1
rm ./keyboard-switch/libuiohook.so.1.2.0
rm ./keyboard-switch/icon.icns

mv ./keyboard-switch/appsettings.linux.json ./keyboard-switch/appsettings.json

cp ../build/rpm/keyboard-switch.spec .
cp ../LICENSE .

rpmbuild -bb --build-in-place --define "_topdir $(pwd)/rpm" keyboard-switch.spec
mv rpm/RPMS/*/*.rpm .
rm -rf rpm
