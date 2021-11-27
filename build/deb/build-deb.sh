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

rm ./keyboard-switch_4.0-1_amd64.deb 2> /dev/null
rm -rf ./keyboard-switch_4.0-1_amd64 2> /dev/null

mkdir keyboard-switch_4.0-1_amd64
cd keyboard-switch_4.0-1_amd64

mkdir DEBIAN
cd DEBIAN

cp ../../../build/deb/control .
cp ../../../build/deb/postinst .
cp ../../../build/deb/prerm .
cp ../../../build/deb/postrm .

sudo chmod 555 ./postinst ./prerm

cd ..
mkdir ./opt
cp -r ../keyboard-switch ./opt

mkdir -p ./usr/share/doc/keyboard-switch
cp ../../build/deb/copyright ./usr/share/doc/keyboard-switch

mkdir -p ./usr/share/icons/hicolor/512x512/app
cp ./opt/keyboard-switch/icon.png ./usr/share/icons/hicolor/512x512/app
mv ./usr/share/icons/hicolor/512x512/app/icon.png ./usr/share/icons/hicolor/512x512/app/keyboard-switch.png
sudo chmod 644 ./usr/share/icons/hicolor/512x512/app/keyboard-switch.png

cd ..
dpkg-deb --build --root-owner-group keyboard-switch_4.0-1_amd64
rm -rf keyboard-switch_4.0-1_amd64
