Name: keyboard-switch
Version: $VERSION
Release: $RELEASE
Summary: Switches typed text as if it were typed with another keyboard layout

License: MIT
Packager: Tolik Pylypchuk <pylypchuk.tolik@gmail.com>
URL: https://keyboardswitch.tolik.io

ExclusiveArch: $ARCH
Requires: libXtst
AutoReqProv: no

%description
Switches typed text as if it were typed with another keyboard layout

%prep

mkdir -p $RPM_BUILD_ROOT/opt/keyboard-switch
cp -r $OUTPUT/* $RPM_BUILD_ROOT/opt/keyboard-switch/

exit

%clean

rm -rf $RPM_BUILD_ROOT/opt/keyboard-switch

%files

%license LICENSE
%attr(0755, root, root) /opt/keyboard-switch/*

%pre

%post

INSTALL_DIR=/opt/keyboard-switch
SETTINGS_APP=$INSTALL_DIR/KeyboardSwitchSettings
SETTINGS_DESKTOP_FILE=/tmp/keyboard-switch-settings.desktop

GNOME_EXTENSION_DIR=/usr/share/gnome-shell/extensions/switch-layout@tolik.io

echo "[Desktop Entry]
Version=1.0
Name=Keyboard Switch Settings
Comment=Switches typed text as if it were typed with another keyboard layout
Exec=$SETTINGS_APP
Path=$INSTALL_DIR
Icon=$INSTALL_DIR/keyboard-switch.png
Terminal=false
Type=Application
Categories=Utility
" | tee -a $SETTINGS_DESKTOP_FILE > /dev/null

desktop-file-install --dir=/usr/share/applications $SETTINGS_DESKTOP_FILE
rm $SETTINGS_DESKTOP_FILE

command -v gnome-shell &> /dev/null

if [ "$?" = 0 ]
then
    mkdir -p $GNOME_EXTENSION_DIR
    cp $INSTALL_DIR/{extension.js,metadata.json} $GNOME_EXTENSION_DIR
fi

%preun

INSTALL_DIR=/opt/keyboard-switch

SERVICE_DESKTOP_FILE=.config/autostart/keyboard-switch.desktop
SETTINGS_DESKTOP_FILE=/usr/share/applications/keyboard-switch-settings.desktop

GNOME_EXTENSION_DIR=/usr/share/gnome-shell/extensions/switch-layout@tolik.io

$INSTALL_DIR/KeyboardSwitch --stop

awk -F: '($3 >= 1000) && ($3 < 60000) && ($1 != "nobody") { print $1 }' /etc/passwd | while read -r CURRENT_USER
do
    if [ -f "$(eval echo ~$CURRENT_USER)/.keyboard-switch/.setup-configured" ]
    then
        rm "$(eval echo ~$CURRENT_USER)/.keyboard-switch/.setup-configured"
    fi

    if [ -f "$(eval echo ~$CURRENT_USER)/$SERVICE_DESKTOP_FILE" ]
    then
        rm "$(eval echo ~$CURRENT_USER)/$SERVICE_DESKTOP_FILE"
    fi
done

if [ -f "$SETTINGS_DESKTOP_FILE" ]
then
    rm "$SETTINGS_DESKTOP_FILE"
    update-desktop-database
fi

if [ -d $GNOME_EXTENSION_DIR ]
then
    rm -rf $GNOME_EXTENSION_DIR
fi

%postun

rm -rf /opt/keyboard-switch
