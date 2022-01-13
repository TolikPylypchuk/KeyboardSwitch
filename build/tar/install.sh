#!/bin/bash

INSTALL_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

SERVICE_APP=$INSTALL_DIR/KeyboardSwitch
SETTINGS_APP=$INSTALL_DIR/KeyboardSwitchSettings

SERVICE_DESKTOP_FILE=$HOME/.config/autostart/keyboard-switch.desktop
SETTINGS_DESKTOP_FILE=$HOME/keyboard-switch-settings.desktop

mkdir -p $HOME/.config/autostart

echo "[Desktop Entry]
Version=1.0
Name=Keyboard Switch
Comment=Switches typed text as if it were typed with another keyboard layout
Exec=$SERVICE_APP
TryExec=$SERVICE_APP
Path=$INSTALL_DIR
Icon=$INSTALL_DIR/icon.png
Terminal=false
Type=Application
Categories=Utility
" | tee -a $SERVICE_DESKTOP_FILE > /dev/null

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
