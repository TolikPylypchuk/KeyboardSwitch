#!/usr/bin/env bash

INSTALL_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

SERVICE_APP=$INSTALL_DIR/KeyboardSwitch
SETTINGS_APP=$INSTALL_DIR/KeyboardSwitchSettings
SETTINGS_DESKTOP_FILE=$HOME/keyboard-switch-settings.desktop

GNOME_EXTENSION_DIR=$HOME/.local/share/gnome-shell/extensions/switch-layout@tolik.io
GNOME_EXTENSION_SOURCE_DIR=$INSTALL_DIR/gnome-extension

GROUP=keyboard-switch

getent group $GROUP &>/dev/null || sudo groupadd --system $GROUP

sudo chown root:$GROUP $SERVICE_APP
sudo chmod g+s $SERVICE_APP

echo "SUBSYSTEM==\"input\", KERNEL==\"event*\", RUN+=\"/usr/bin/setfacl -m g:$GROUP:rw \$env{DEVNAME}\"
KERNEL==\"uinput\", RUN+=\"/usr/bin/setfacl -m g:$GROUP:rw \$env{DEVNAME}\"
" | sudo tee /etc/udev/rules.d/70-keyboard-switch.rules > /dev/null

sudo udevadm control --reload-rules && sudo udevadm trigger

echo "[Desktop Entry]
Version=1.0
Name=Keyboard Switch Settings
Comment=Switches typed text as if it were typed with another keyboard layout
Exec=$SETTINGS_APP
TryExec=$SETTINGS_APP
Path=$INSTALL_DIR
Icon=$INSTALL_DIR/keyboard-switch.png
Terminal=false
Type=Application
Categories=Utility
" | tee -a $SETTINGS_DESKTOP_FILE > /dev/null

desktop-file-install --dir=$HOME/.local/share/applications $SETTINGS_DESKTOP_FILE
rm $SETTINGS_DESKTOP_FILE

if command -v gnome-shell &> /dev/null
then
    GNOME_VERSION=$(gnome-shell --version | grep -oE '[0-9]+' | head -1)

    if [ -n "$GNOME_VERSION" ] && [ "$GNOME_VERSION" -ge 45 ]
    then
        GNOME_EXTENSION_VERSION_DIR=v2
    else
        GNOME_EXTENSION_VERSION_DIR=v1
    fi

    if [ -d "$GNOME_EXTENSION_SOURCE_DIR/$GNOME_EXTENSION_VERSION_DIR" ]
    then
        mkdir -p $GNOME_EXTENSION_DIR

        install -m 644 $GNOME_EXTENSION_SOURCE_DIR/$GNOME_EXTENSION_VERSION_DIR/extension.js $GNOME_EXTENSION_DIR
        install -m 644 $GNOME_EXTENSION_SOURCE_DIR/$GNOME_EXTENSION_VERSION_DIR/metadata.json $GNOME_EXTENSION_DIR

        echo "The Switch Layout extension for GNOME has been installed - log out and log back in to let GNOME Shell load it"
    fi
fi
