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
        ARCH="aarch64"
        ;;
    *)
        RUNTIME="linux-x64"
        MSBUILD_PLATFORM="x64"
        ARCH="x86_64"
        ;;
esac

dotnet publish ../KeyboardSwitch --configuration Release --runtime "$RUNTIME" --framework net8.0 \
--self-contained true --output ./keyboard-switch --nologo -p:Platform="$MSBUILD_PLATFORM" \
-p:ContinuousIntegrationBuild=true

dotnet publish ../KeyboardSwitch.Settings --configuration Release --runtime "$RUNTIME" --framework net8.0 \
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

cp ../build/rpm/keyboard-switch.spec .
cp ../LICENSE .

sed -i "s/%ARCH%/$ARCH/g" ./keyboard-switch.spec

rpmbuild -bb --build-in-place --define "_topdir $(pwd)/rpm" --target "$ARCH" keyboard-switch.spec
mv rpm/RPMS/*/*.rpm .

rm -rf rpm
rm keyboard-switch.spec
rm LICENSE
