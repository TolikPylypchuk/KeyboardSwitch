# Installation

## Windows

### Using a Windows Installer

You can get the latest version of the app from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the _.msi_ file and run it to install the app. If your computer runs on the x64 architecture, then you should get the _x64_ installer. If your computer runs on the Arm architecture, then you should get the _arm64_ installer. If you're not sure, then you most likely need the _x64_ installer.

After installing the app, the installer will start the settings app. On the first start-up the settings app will configure the service to run when you log into the system.

When the settings app starts up you can configure the character mappings (you can read more about it in the next article) and other preferences. Start the app using the _Start_ button at the bottom of the window. That's it! Now you're ready to use the Keyboard Switch service.

### Uninstalling the App

You can uninstall the app using Windows settings just like you would uninstall any other app.

{% hint style="warning" %}
The uninstaller will not delete the app's configuration. If you want to delete it, then go to your local app data folder, and delete the _KeyboardSwitch_ folder.

The uninstaller will also not delete the registry entry which says that your app should start when you log in. If you want to keep your registry clean, you have to disable it in the settings app before uninstalling it.
{% endhint %}

### Using the Portable Version

If you don't want to install the app (or can't), you can use the portable version of the app. It's literally the same as the installable version; there are no differences. Again, go to the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases) and download _KeyboardSwitch-4.2.0-x64-win.zip_ or _KeyboardSwitch-4.2.0-arm64-win.zip_. If you're not sure which one you should download, then it's most probably _x64_. Extract the archive to anywhere you want and start _KeyboardSwitchSettings.exe_. It will configure the app to run when you log in just like the installed version. The configuration for the portable version is also stored under your local app data folder.

## macOS

### Installing the App

You can get the latest version of the app from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the _.pkg_ file and run it to install the app. If your computer runs on an Apple CPU (e.g. M1), then you should get the _arm64_ package. If your computer runs on an Intel CPU, then you should get the _x86\_64_ package.

The installer will install multiple things:

* The Keyboard Switch Service app will be put into the _/Library/Application Support_ folder — you shouldn't run or interact with this app directly
* The Keyboard Switch Settings app will be put into the _/Applications_ folder
* The descriptor file for running the service app when you log in will be put into the _/Library/LaunchAgents_ folder

After installing the app, open Keyboard Switch Settings — it should appear in the list of your apps.

Immediately upon opening the app, a dialog window should appear which says that Keyboard Switch would like to control this computer using accessibility features. The app needs this to listen to the magic key combination while running in the background, and without these permissions it won't work. This dialog will appear only when you're installing the app for the first time. If you have installed it previously, then macOS will most probably remember that you gave the app appropriate permissions.

Click the _Open System Preferences_ button on the dialog window. Enable the _Keyboard Switch_ item. Close System Preferences and go back to the Keyboard Switch Settings app.

If the dialog window didn't appear for some reason, then press the _Start_ button at the bottom of the window. The service app will start and immediately stop, because it doesn't have the accessibility permissions, and the dialog window will appear again.

You can configure the character mappings (you can read more about it in the next article) and other preferences. Start the app using the _Start_ button. That's it! Now you're ready to use the Keyboard Switch service.

### Uninstalling the App

Many apps on macOS can be uninstalled just by deleting the app bundle from the _/Applications_ folder. This is not the case with Keyboard Switch. Multiple things should be done to remove it from the system. You shouldn't concern yourself with those things though — you should just run an uninstaller package, and it will remove Keyboard Switch.

As with the installer package, you can also get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download and run the _KeyboardSwitchUninstaller-4.2.0.pkg_ file, and Keyboard Switch will be removed.

If you want to delete the app's configuration as well, then delete the _\[home]/Library/Application Support/Keyboard Switch_ folder. You can also delete the app's log files by deleting the _\[home]/Library/Logs/Keyboard Switch_ folder.

## Linux

### Installing the App

#### Prerequisites

There are several prerequisites for running the app on Linux:

* X11
* X Keyboard Extension (XKB) which is enabled by default
* X Test Extension — used to simulate pressing keys like <kbd>Ctrl</kbd>+<kbd>C</kbd> and <kbd>Ctrl</kbd>+<kbd>V</kbd> for you
* [xsel](https://github.com/kfish/xsel) (optional) — used to copy and paste text if the&#x20;
* Freedesktop conventions — used to make the service app start when you log in, and to make the settings app appear in the list of your installed apps (not required for the app itself though)

Keyboard Switch doesn't support Wayland (even with XWayland).

The most popular desktop systems (at least GNOME, KDE Plasma, Cinnamon, LXQt, and Xfce) all adhere to the Freedesktop protocols, so the last prerequisite is automatically available, unless you're running a very unusual setup.

Starting with version 4.2, Keyboard Switch uses native clipboard integration by default. This works well on newer desktop environments but doesn't work well on older versions. If you notice that Keyboard Switch doesn't work well on your system, then you can switch to using xsel instead. xsel must be installed manually though, and the service app must be restarted when this setting is changed.

[Click here](https://github.com/TolikPylypchuk/KeyboardSwitch/issues/94) to see the list of Linux distributions on which the app was tested.

{% hint style="warning" %}
If your desktop environment is GNOME, then you should restart it right after opening the settings app for the first time. If you're not sure which desktop environment you're using then it's most probably GNOME since it's the default one on Ubuntu, Debian, CentOS, Fedora and others (but not Linux Mint). Press <kbd>Alt</kbd>+<kbd>F2</kbd>, then type <kbd>r</kbd> and press <kbd>Enter</kbd>. This will restart GNOME.

This is not required though, but if you don't restart GNOME, then Keyboard Switch won't be able to switch your keyboard layout until you re-login or reboot your system.
{% endhint %}

#### Using a Deb Package

If you're running a Debian-based distribution (e.g., Ubuntu or Mint), then you can install Keyboard Switch using a deb package. You can get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

The package takes care of the X Test extension, so you don't need to install it yourself.

After installation, the settings app will be available in the list of installed apps, and the service app will be configured to run at login.

Both the _amd64_ and _arm64_ versions are available.

#### Using an RPM Package

If you're running a RHEL-based distribution (e.g., Rocky Linux or Fedora), or SUSE, then you can install Keyboard Switch using an RPM package. You can get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

The package takes care of the X Test extension, so you don't need to install it yourself.

{% hint style="info" %}
If you're using RHEL or Rocky Linux, then make sure that the EPEL repository is installed before installing Keyboard Switch. Otherwise, it won't be able to install xsel.
{% endhint %}

{% hint style="info" %}
SUSE may complain that it cannot find libXtst when installing the app using the RPM package. This is because the package is targeted for RHEL and derivatives. You can ignore this warning and proceed with installation, but make sure that libXtst6 is installed before starting the app.
{% endhint %}

After installation, the settings app will be available in the list of installed apps, and the service app will be configured to run at login.

Both the _x86\_64_ and _aarch64_ versions are available.

#### Using a Tar Archive

Keyboard Switch is also available as a _tar.gz_ file. You can get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). You can then extract it to anywhere you like (e.g., into the _/opt_ directory). The deployed app includes two scripts — _install.sh_ and _uninstall.sh_. _install.sh_ adds the settings app to the list of installed apps. _uninstall.sh_ deletes this configuration. These scripts will make changes only for the current user.

Both the _x64_ and _arm64_ versions are available.

The following scripts assume that the _/opt_ directory is writable. Substitute _x64_ with _arm64_ if needed.

Here are the steps required for installing the app on Debian, Ubuntu, Linux Mint, etc.:

```
sudo apt update
sudo apt install libxtst6  # Install the X Test Extension
tar -xzf keyboard-switch-4.2.0-x64.tar.gz -C /opt
cd /opt/keyboard-switch
./install.sh
```

Here are the steps required for installing the app on RHEL, Fedora, or Rocky Linux:

```
sudo dnf install epel-release  # Add the EPEL repository (not needed for Fedora)
sudo dnf install libXtst       # Install the X Test Extension
tar -xzf keyboard-switch-4.2.0-x64.tar.gz -C /opt
cd /opt/keyboard-switch
./install.sh
```

Here are the steps required for installing the app on SUSE (you may need to add the X11:XOrg repository to install libXtst6):

```
sudo zypper install libXtst6  # Install the X Test Extension
tar -xzf keyboard-switch-4.2.0-x64.tar.gz -C /opt
cd /opt/keyboard-switch
./install.sh
```

Here are the steps required for installing the app on Arch Linux:

```
sudo pacman -S libxtst  # Install the X Test Extension
tar -xzf keyboard-switch-4.2.0-x64.tar.gz -C /opt
cd /opt/keyboard-switch
./install.sh
```

### Uninstalling the App

For the deb and RPM packages, it's simple — just uninstall the package. For the _tar.gz_ file, run the included _uninstall.sh_ script, and delete the app's directory.

If you want to delete the app's configuration as well, then there are several ways to do that:

* For deb packages, you can run `apt remove` with the `--purge` option.
* For _tar.gz_ files, you can run `uninstall.sh` with the `--purge` option.
* You can delete the _\~/.config/keyboard-switch_ directory manually.

There is no way to delete the app configuration automatically for RPM packages.
