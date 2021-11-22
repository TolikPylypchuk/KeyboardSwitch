Name: keyboard-switch
Version: 4.0
Release: 1
Summary: An application which switches typed text as if it were typed with another keyboard layout

License: MIT
Packager: Tolik Pylypchuk <pylypchuk.tolik@gmail.com>
URL: https://keyboardswitch.tolik.io

ExclusiveArch: x86_64
Requires: xsel libXtst

%description
An application which switches typed text as if it were typed with another keyboard layout

%prep

mkdir -p $RPM_BUILD_ROOT/opt/keyboard-switch
cp -r ./keyboard-switch/* $RPM_BUILD_ROOT/opt/keyboard-switch/

exit

%clean

rm -rf $RPM_BUILD_ROOT/opt/keyboard-switch

%files

%license LICENSE
%attr(0755, root, root) /opt/keyboard-switch/*

%pre

%post

INSTALL_DIR=/opt/keyboard-switch
AUTOSTART_DIR=.config/autostart

SERVICE_APP=$INSTALL_DIR/KeyboardSwitch
SETTINGS_APP=$INSTALL_DIR/KeyboardSwitchSettings

SERVICE_DESKTOP_FILE=$AUTOSTART_DIR/keyboard-switch.desktop
SETTINGS_DESKTOP_FILE=/tmp/keyboard-switch-settings.desktop

awk -F: '($3 >= 1000) && ($3 < 60000) && ($1 != "nobody") { print $1 }' /etc/passwd | while read -r CURRENT_USER
do
    mkdir -p "$(eval echo ~$CURRENT_USER)/$AUTOSTART_DIR"

    echo "[Desktop Entry]
Version=1.0
Name=Keyboard Switch
Comment=An application to switch typed text as if it were typed with another keyboard layout
Exec=$SERVICE_APP
TryExec=$SERVICE_APP
Path=$INSTALL_DIR
Icon=$INSTALL_DIR/icon.png
Terminal=false
Type=Application
Categories=Utility
" | tee -a "$(eval echo ~$CURRENT_USER)/$SERVICE_DESKTOP_FILE" > /dev/null
done

echo "[Desktop Entry]
Version=1.0
Name=Keyboard Switch Settings
Comment=An application to switch typed text as if it were typed with another keyboard layout
Exec=$SETTINGS_APP
Path=$INSTALL_DIR
Icon=$INSTALL_DIR/icon.png
Terminal=false
Type=Application
Categories=Utility
" | tee -a $SETTINGS_DESKTOP_FILE > /dev/null

desktop-file-install --dir=/usr/share/applications $SETTINGS_DESKTOP_FILE
rm $SETTINGS_DESKTOP_FILE

%preun

INSTALL_DIR=/opt/keyboard-switch

SERVICE_DESKTOP_FILE=.config/autostart/keyboard-switch.desktop
SETTINGS_DESKTOP_FILE=/usr/share/applications/keyboard-switch-settings.desktop

$INSTALL_DIR/KeyboardSwitch --stop

awk -F: '($3 >= 1000) && ($3 < 60000) && ($1 != "nobody") { print $1 }' /etc/passwd | while read -r CURRENT_USER
do
    if [ -f "$(eval echo ~$CURRENT_USER)/$SERVICE_DESKTOP_FILE" ] ; then
        rm "$(eval echo ~$CURRENT_USER)/$SERVICE_DESKTOP_FILE"
    fi
done

if [ -f "$SETTINGS_DESKTOP_FILE" ] ; then
    rm "$SETTINGS_DESKTOP_FILE"
    update-desktop-database
fi

%postun

rm -rf /opt/keyboard-switch
