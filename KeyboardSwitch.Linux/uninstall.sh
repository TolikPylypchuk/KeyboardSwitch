#!/bin/bash

SYSTEMD_SERVICE=keyboard-switch
UNIT_FILE=/etc/systemd/user/$SYSTEMD_SERVICE.service

systemctl --user disable $SYSTEMD_SERVICE
systemctl --user stop $SYSTEMD_SERVICE

sudo rm $UNIT_FILE

systemctl --user daemon-reload

DESKTOP_FILE=$HOME/.local/share/applications/keyboard-switch.desktop

if [ -f "$DESKTOP_FILE" ] ; then
    rm "$DESKTOP_FILE"
    sudo update-desktop-database
fi
