#!/bin/bash

SYSTEMD_SERVICE=keyboard-switch
UNIT_FILE=/etc/systemd/user/$SYSTEMD_SERVICE.service
SERVICE_APP=$PWD/KeyboardSwitch

sudo echo "[Unit]
Description=Keyboard Switch
Documentation=https://docs.keyboardswitch.tolik.io

[Service]
Type=notify
ExecStart=$SERVICE_APP
ExecStop=$SERVICE_APP --stop
ExecReload=$SERVICE_APP --reload
Environment=\"DISPLAY=:0\"

[Install]
WantedBy=default.target
" | sudo tee -a $UNIT_FILE > /dev/null

systemctl --user daemon-reload
systemctl --user enable $SYSTEMD_SERVICE
