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
-p:PublishSingleFile=true -p:ContinuousIntegrationBuild=true

dotnet publish ../KeyboardSwitch.Settings --configuration Release --runtime "$RUNTIME" --framework net6.0 \
--self-contained true --output ./keyboard-switch --nologo -p:Platform="$MSBUILD_PLATFORM" \
-p:PublishSingleFile=true -p:ContinuousIntegrationBuild=true

rm ./keyboard-switch/appsettings.windows.json
rm ./keyboard-switch/appsettings.linux.json
rm ./keyboard-switch/libuiohook.1.dylib
rm ./keyboard-switch/libuiohook.1.2.0.dylib
rm ./keyboard-switch/icon.png

mv ./keyboard-switch/appsettings.macos.json ./keyboard-switch/appsettings.json
mv ./keyboard-switch/icon.icns ./keyboard-switch/KeyboardSwitch.icns
cp ../build/macos/entitlements.plist .
cp ../build/macos/dist-pkg.xml .

mkdir scripts
cp ../build/macos/postinstall-pkg scripts/
mv scripts/postinstall-pkg scripts/postinstall

mkdir resources
cp ../build/macos/license.txt resources/
cp ../build/macos/readme.txt resources/

mkdir "Keyboard Switch.app"
mkdir "Keyboard Switch.app/Contents"
mkdir "Keyboard Switch.app/Contents/MacOS"
mkdir "Keyboard Switch.app/Contents/Resources"

cp ./keyboard-switch/KeyboardSwitch "Keyboard Switch.app/Contents/MacOS/"
cp ./keyboard-switch/libe_sqlite3.dylib "Keyboard Switch.app/Contents/MacOS/"
cp ./keyboard-switch/libuiohook.dylib "Keyboard Switch.app/Contents/MacOS/"

cp ./keyboard-switch/appsettings.json "Keyboard Switch.app/Contents/Resources/"
cp ./keyboard-switch/KeyboardSwitch.icns "Keyboard Switch.app/Contents/Resources/"

cp ../build/macos/KeyboardSwitch.plist "Keyboard Switch.app/Contents/"
mv "Keyboard Switch.app/Contents/KeyboardSwitch.plist" \
"Keyboard Switch.app/Contents/Info.plist"

cp ../build/macos/io.tolik.keyboardswitch.plist "Keyboard Switch.app/Contents/Resources/"

sed -i '' "s/%ARCH%/$ARCH/g" "Keyboard Switch.app/Contents/Info.plist"

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

codesign --remove-signature "Keyboard Switch.app/Contents/MacOS/KeyboardSwitch"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
"Keyboard Switch.app/Contents/MacOS/libe_sqlite3.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
"Keyboard Switch.app/Contents/MacOS/libuiohook.dylib"

codesign --sign "$APPLE_APPLICATION_CERTIFICATE" --timestamp --no-strict --entitlements entitlements.plist \
--options=runtime "Keyboard Switch.app/Contents/MacOS/KeyboardSwitch"

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

sed -i '' "s/%ARCH%/$ARCH/g" dist-pkg.xml

pkgbuild --component "Keyboard Switch.app" --identifier io.tolik.keyboardswitch.service --version 4.2.0 \
--install-location /opt --scripts scripts KeyboardSwitch.pkg

pkgbuild --component "Keyboard Switch Settings.app" --identifier io.tolik.keyboardswitch.settings --version 4.2.0 \
--install-location /Applications KeyboardSwitchSettings.pkg

productbuild --sign "$APPLE_INSTALLER_CERTIFICATE" --distribution dist-pkg.xml --resources resources \
"KeyboardSwitch-4.2-$ARCH.pkg"

rm KeyboardSwitch.pkg
rm KeyboardSwitchSettings.pkg

xcrun notarytool submit "KeyboardSwitch-4.2-$ARCH.pkg" --wait \
--apple-id "$APPLE_ID" --team-id "$APPLE_TEAM_ID" --password "$NOTARIZATION_PASSWORD"

xcrun stapler staple "KeyboardSwitch-4.2-$ARCH.pkg"
