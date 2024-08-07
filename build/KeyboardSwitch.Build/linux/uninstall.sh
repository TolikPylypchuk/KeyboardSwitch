#!/usr/bin/env bash

INSTALL_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

SERVICE_DESKTOP_FILE=$HOME/.config/autostart/keyboard-switch.desktop
SETTINGS_DESKTOP_FILE=$HOME/.local/share/applications/keyboard-switch-settings.desktop

GNOME_EXTENSION_DIR=$HOME/.local/share/gnome-shell/extensions/switch-layout@tolik.io

$INSTALL_DIR/KeyboardSwitch --stop

if [ -f "$HOME/.config/keyboard-switch/.setup-configured" ]
then
    rm "$HOME/.config/keyboard-switch/.setup-configured"
fi

if [ "$1" == "--purge" ]
then
    if [ -d "$HOME/.config/keyboard-switch" ]
    then
        rm -rf "$HOME/.config/keyboard-switch"
    fi
fi

if [ -f "$SERVICE_DESKTOP_FILE" ]
then
    rm "$SERVICE_DESKTOP_FILE"
fi

if [ -f "$SETTINGS_DESKTOP_FILE" ]
then
    rm "$SETTINGS_DESKTOP_FILE"
    sudo update-desktop-database
fi

if [ -d $GNOME_EXTENSION_DIR ]
then
    rm -rf $GNOME_EXTENSION_DIR
fi
