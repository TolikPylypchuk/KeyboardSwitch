#!/bin/bash

INSTALL_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

SYSTEMD_SERVICE=keyboard-switch
UNIT_FILE=/etc/systemd/user/$SYSTEMD_SERVICE.service

SERVICE_APP=$INSTALL_DIR/KeyboardSwitch
SETTINGS_APP=$INSTALL_DIR/KeyboardSwitchSettings

DESKTOP_FILE=$HOME/keyboard-switch.desktop

sudo echo "[Unit]
Description=Keyboard Switch
Documentation=https://docs.keyboardswitch.tolik.io

[Service]
Type=notify
ExecStart=$SERVICE_APP --retry 5s,10s,15s
ExecStop=$SERVICE_APP --stop
ExecReload=$SERVICE_APP --reload-settings
Environment=\"DISPLAY=:0\"

[Install]
WantedBy=default.target
" | sudo tee -a $UNIT_FILE > /dev/null

systemctl --user daemon-reload
systemctl --user enable $SYSTEMD_SERVICE

if [ "$0" = 0 -o "$1" != "--skip-desktop" ]; then
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
" | tee -a $DESKTOP_FILE > /dev/null

    desktop-file-install --dir=$HOME/.local/share/applications $DESKTOP_FILE
    rm $DESKTOP_FILE
fi
