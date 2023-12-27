#!/bin/bash

INSTALL_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

SETTINGS_APP=$INSTALL_DIR/KeyboardSwitchSettings
SETTINGS_DESKTOP_FILE=$HOME/keyboard-switch-settings.desktop

GNOME_EXTENSION_DIR=$HOME/.local/share/gnome-shell/extensions/switch-layout@tolik.io

echo "[Desktop Entry]
Version=1.0
Name=Keyboard Switch Settings
Comment=Switches typed text as if it were typed with another keyboard layout
Exec=$SETTINGS_APP
TryExec=$SETTINGS_APP
Path=$INSTALL_DIR
Icon=$INSTALL_DIR/icon.png
Terminal=false
Type=Application
Categories=Utility
" | tee -a $SETTINGS_DESKTOP_FILE > /dev/null

desktop-file-install --dir=$HOME/.local/share/applications $SETTINGS_DESKTOP_FILE
rm $SETTINGS_DESKTOP_FILE

command -v gnome-shell &> /dev/null

if [ "$?" = 0 ]
then
    mkdir -p $GNOME_EXTENSION_DIR
    cp $INSTALL_DIR/{extension.js,metadata.json} $GNOME_EXTENSION_DIR
fi
