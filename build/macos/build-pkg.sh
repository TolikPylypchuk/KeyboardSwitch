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
        RUNTIME="osx-arm64"
        MSBUILD_PLATFORM="ARM64"
        ARCH="arm64"
        ;;
    *)
        RUNTIME="osx-x64"
        MSBUILD_PLATFORM="x64"
        ARCH="x86_64"
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
rm ./keyboard-switch/appsettings.linux.json
rm ./keyboard-switch/libuiohook.1.dylib
rm ./keyboard-switch/libuiohook.1.2.0.dylib
rm ./keyboard-switch/icon.png

mv ./keyboard-switch/appsettings.macos.json ./keyboard-switch/appsettings.json
mv ./keyboard-switch/icon.icns ./keyboard-switch/KeyboardSwitch.icns
cp ../build/macos/entitlements.plist .

mkdir "Keyboard Switch Service.app"
mkdir "Keyboard Switch Service.app/Contents"
mkdir "Keyboard Switch Service.app/Contents/MacOS"
mkdir "Keyboard Switch Service.app/Contents/Resources"

cp ./keyboard-switch/KeyboardSwitch "Keyboard Switch Service.app/Contents/MacOS/"
cp ./keyboard-switch/libe_sqlite3.dylib "Keyboard Switch Service.app/Contents/MacOS/"
cp ./keyboard-switch/libuiohook.dylib "Keyboard Switch Service.app/Contents/MacOS/"

cp ./keyboard-switch/appsettings.json "Keyboard Switch Service.app/Contents/Resources/"
cp ./keyboard-switch/KeyboardSwitch.icns "Keyboard Switch Service.app/Contents/Resources/"

cp ../build/macos/KeyboardSwitchService.plist "Keyboard Switch Service.app/Contents/"
mv "Keyboard Switch Service.app/Contents/KeyboardSwitchService.plist" \
"Keyboard Switch Service.app/Contents/Info.plist"

sed -i '' "s/%ARCH%/$ARCH/g" "Keyboard Switch Service.app/Contents/Info.plist"

mkdir "Keyboard Switch Settings.app"
mkdir "Keyboard Switch Settings.app/Contents"
mkdir "Keyboard Switch Settings.app/Contents/MacOS"
mkdir "Keyboard Switch Settings.app/Contents/Resources"

cp ./keyboard-switch/KeyboardSwitchSettings "Keyboard Switch Settings.app/Contents/MacOS/"
cp ./keyboard-switch/libAvaloniaNative.dylib "Keyboard Switch Settings.app/Contents/MacOS/"
cp ./keyboard-switch/libe_sqlite3.dylib "Keyboard Switch Settings.app/Contents/MacOS/"
cp ./keyboard-switch/libHarfBuzzSharp.dylib "Keyboard Switch Settings.app/Contents/MacOS/"
cp ./keyboard-switch/libSkiaSharp.dylib "Keyboard Switch Settings.app/Contents/MacOS/"

cp ./keyboard-switch/appsettings.json "Keyboard Switch Settings.app/Contents/Resources/"
cp ./keyboard-switch/KeyboardSwitch.icns "Keyboard Switch Settings.app/Contents/Resources/"

cp ../build/macos/KeyboardSwitchSettings.plist "Keyboard Switch Settings.app/Contents/"
mv "Keyboard Switch Settings.app/Contents/KeyboardSwitchSettings.plist" \
"Keyboard Switch Settings.app/Contents/Info.plist"

sed -i '' "s/%ARCH%/$ARCH/g" "Keyboard Switch Settings.app/Contents/Info.plist"

codesign --remove-signature "Keyboard Switch Service.app/Contents/MacOS/KeyboardSwitch"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
"Keyboard Switch Service.app/Contents/MacOS/libe_sqlite3.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
"Keyboard Switch Service.app/Contents/MacOS/libuiohook.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
--options=runtime "Keyboard Switch Service.app/Contents/MacOS/KeyboardSwitch"

codesign --remove-signature "Keyboard Switch Settings.app/Contents/MacOS/KeyboardSwitchSettings"
codesign --remove-signature "Keyboard Switch Settings.app/Contents/MacOS/libAvaloniaNative.dylib"
codesign --remove-signature "Keyboard Switch Settings.app/Contents/MacOS/libHarfBuzzSharp.dylib"
codesign --remove-signature "Keyboard Switch Settings.app/Contents/MacOS/libSkiaSharp.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
"Keyboard Switch Settings.app/Contents/MacOS/libAvaloniaNative.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
"Keyboard Switch Settings.app/Contents/MacOS/libe_sqlite3.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
"Keyboard Switch Settings.app/Contents/MacOS/libHarfBuzzSharp.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
"Keyboard Switch Settings.app/Contents/MacOS/libSkiaSharp.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
--options=runtime "Keyboard Switch Settings.app/Contents/MacOS/KeyboardSwitchSettings"

productbuild --sign "$APPLE_INSTALLER_CERTIFICATE" --component "Keyboard Switch Service.app" /opt \
--component "Keyboard Switch Settings.app" /Applications "KeyboardSwitch-4.1-$ARCH.pkg"

xcrun notarytool submit "KeyboardSwitch-4.1-$ARCH.pkg" --wait \
--apple-id "$APPLE_ID" --team-id "$APPLE_TEAM_ID" --password "$NOTARIZATION_PASSWORD"

xcrun stapler staple "KeyboardSwitch-4.1-$ARCH.pkg"
