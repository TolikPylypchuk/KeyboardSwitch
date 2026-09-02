#!/usr/bin/env bash

INSTALL_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

SERVICE_APP=$INSTALL_DIR/KeyboardSwitch
SETTINGS_APP=$INSTALL_DIR/KeyboardSwitchSettings
SETTINGS_DESKTOP_FILE=$HOME/keyboard-switch-settings.desktop

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
