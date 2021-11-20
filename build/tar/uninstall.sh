#!/bin/bash

INSTALL_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

SERVICE_DESKTOP_FILE=$HOME/.config/autostart/keyboard-switch.desktop
SETTINGS_DESKTOP_FILE=$HOME/.local/share/applications/keyboard-switch-settings.desktop

$INSTALL_DIR/KeyboardSwitch --stop

if [ -f "$SERVICE_DESKTOP_FILE" ] ; then
    rm "$SERVICE_DESKTOP_FILE"
fi

if [ -f "$SETTINGS_DESKTOP_FILE" ] ; then
    rm "$SETTINGS_DESKTOP_FILE"
    sudo update-desktop-database
fi
