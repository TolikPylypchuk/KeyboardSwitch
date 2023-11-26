#!/bin/bash

mkdir -p ./bin
cd ./bin

cp ../build/macos/dist-uninstaller-pkg.xml .

mkdir scripts
cp ../build/macos/postinstall-uninstaller-pkg scripts/
mv scripts/postinstall-uninstaller-pkg scripts/postinstall

mkdir resources
cp ../build/macos/welcome.txt resources/

pkgbuild --nopayload --identifier io.tolik.keyboardswitch.uninstaller --scripts scripts --version 4.2.0 \
KeyboardSwitchUninstaller.pkg

productbuild --sign "$APPLE_INSTALLER_CERTIFICATE" --distribution dist-uninstaller-pkg.xml --resources resources \
KeyboardSwitchUninstaller-4.2.pkg

rm KeyboardSwitchUninstaller.pkg

xcrun notarytool submit KeyboardSwitchUninstaller-4.2.pkg --wait \
--apple-id "$APPLE_ID" --team-id "$APPLE_TEAM_ID" --password "$NOTARIZATION_PASSWORD"

xcrun stapler staple KeyboardSwitchUninstaller-4.2.pkg
