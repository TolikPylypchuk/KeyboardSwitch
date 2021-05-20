#!/bin/bash

SYSTEMD_SERVICE=keyboard-switch
UNIT_FILE=/etc/systemd/user/$SYSTEMD_SERVICE.service

systemctl --user disable $SYSTEMD_SERVICE
systemctl --user stop $SYSTEMD_SERVICE

sudo rm $UNIT_FILE

systemctl --user daemon-reload
