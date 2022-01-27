#!/bin/bash

rm -rf ./bin/keyboard-switch 2> /dev/null
mkdir -p ./bin
cd ./bin

while getopts p: opts; do
   case ${opts} in
      p) PLATFORM=${OPTARG};;
   esac
done

if [ -z "$PLATFORM" ]
then
    PLATFORM="x64"
fi

PLATFORM=$(echo "$PLATFORM" | tr '[:upper:]' '[:lower:]')

case $PLATFORM in
    "arm64")
        RUNTIME="linux-arm64"
        MSBUILD_PLATFORM="ARM64"
        ARCH="arm64"
        ;;
    *)
        RUNTIME="linux-x64"
        MSBUILD_PLATFORM="x64"
        ARCH="amd64"
        ;;
esac

dotnet publish ../KeyboardSwitch --configuration Release --runtime "$RUNTIME" --framework net6.0 \
--self-contained true --output ./keyboard-switch --nologo -p:Platform="$MSBUILD_PLATFORM" \
-p:ContinuousIntegrationBuild=true

dotnet publish ../KeyboardSwitch.Settings --configuration Release --runtime "$RUNTIME" --framework net6.0 \
--self-contained true --output ./keyboard-switch --nologo -p:Platform="$MSBUILD_PLATFORM" \
-p:ContinuousIntegrationBuild=true

find ./keyboard-switch -name "*.pdb" -type f -delete
find ./keyboard-switch -name "*.xml" -type f -delete
rm ./keyboard-switch/appsettings.windows.json
rm ./keyboard-switch/appsettings.macos.json
rm ./keyboard-switch/libuiohook.so.1
rm ./keyboard-switch/libuiohook.so.1.2.0
rm ./keyboard-switch/icon.icns

mv ./keyboard-switch/appsettings.linux.json ./keyboard-switch/appsettings.json

rm ./keyboard-switch_4.1-1_"$ARCH".deb 2> /dev/null
rm -rf ./keyboard-switch_4.1-1_"$ARCH" 2> /dev/null

mkdir keyboard-switch_4.1-1_"$ARCH"
cd keyboard-switch_4.1-1_"$ARCH"

mkdir DEBIAN
cd DEBIAN

cp ../../../build/deb/control .
cp ../../../build/deb/postinst .
cp ../../../build/deb/prerm .
cp ../../../build/deb/postrm .

sed -i "s/%ARCH%/$ARCH/g" ./control

sudo chmod 555 ./postinst ./prerm ./postrm

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
dpkg-deb --build --root-owner-group keyboard-switch_4.1-1_"$ARCH"
rm -rf keyboard-switch_4.1-1_"$ARCH"
