# Installation

{% hint style="warning" %}
Version 4.1 is almost ready, but not yet released. If you want to take it for a spin, then you should download a nightly build from GitHub Actions instead of getting it from the releases page.
{% endhint %}

## Windows

### Using a Windows Installer

You can get the latest version of the app from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the _.msi_ file and run it to install the app.

After installing the app, the installer will run the settings app. Currently it's not really optimized, so the startup time is not perfect; it can take a couple seconds for the app to get started. On the first start-up the settings app will configure the service to run when you log into the system.

When the settings app starts up you should configure the character mappings (you can read more about it in the next article). After you have configured the character mappings and possibly some other preferences, start the app using the _Start_ button at the bottom of the window. That's it! Now you're ready to use the Keyboard Switch service.

Current only the x64 version can be installed with an installer. If you want the Arm64 version then you should use a portable version or [build the installer yourself](../other/technical.md#building-the-windows-installer).

### Uninstalling the App

You can uninstall the app using the Settings app just like you would uninstall any other app.

{% hint style="warning" %}
**Note:** The installer will not delete the registry entry which says that your app should start when you log in! If you want to keep your registry clean, you have to disable it in the settings app before uninstalling it.
{% endhint %}

Upon uninstallation the installer will ask whether you want to delete the app's configuration as well. If you choose not to delete the configuration, you can delete it later manually at any other time. It's stored under the user's local app data folder.

### Using the Portable Version

If you don't want to install the app (or can't), you can use the portable version of the app. It's literally the same as the installable version; there are no differences. Again, go to the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases) and download _KeyboardSwitch-4.1-x64-win.zip_ or _KeyboardSwitch-4.1-arm64-win.zip_. If you're not sure which one you should download, then it's most probably x64. Extract the archive to anywhere you want and start _KeyboardSwitchSettings.exe_. You can configure the app to run when you log in just like the installed version. The configuration for the portable version is also stored under the user's local app data folder.

## macOS

### Installing the App

You can get the latest version of the app from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the _.pkg_ file and run it to install the app. If your computer is running on an Intel CPU then you should get the _x86\_64_ package. If your computer is running on an Apple CPU (e.g. M1) then you should get the _arm64_ package.

The installer will install multiple things:

* The Keyboard Switch Service app will be put into the _/opt_ folder - you shouldn't run or interact with this app directly
* The Keyboard Switch Settings app will be put into the _/Applications_ folder
* The descriptor file for running the service app when you log in will be put into the _/Library/LaunchAgents_ folder

After installing the app, open Keyboard Switch Settings - it should appear in the list of your apps.

Immediately upon opening the app a dialog window should appear which says that Keyboard Switch Service would like to control this computer using accessibility features. The service app needs this to listen to the magic key combination while running in the background, and without these permissions the app won't work.

Click the _Open System Preferences_ button on the dialog window. Unlock the settings and check the _Keyboard Switch Service.app_ checkbox. Lock the settings, close System Preferences, and go back to the Keyboard Switch Settings app.

If the dialog window didn't appear for some reason, then press the _Start_ button at the bottom of the window. The service app will start and immediately crash, because it doesn't have the accessibility permissions, and the dialog window will appear again.

You should configure the character mappings (you can read more about it in the next article). After you have configured the character mappings and possibly some other preferences, start the app using the _Start_ button. That's it! Now you're ready to use the Keyboard Switch service.

### Uninstalling the App

Many apps on macOS can be uninstalled just by deleting the app bundle from the _/Applications_ folder. This is not the case with Keyboard Switch. Multiple things should be done to remove it from the system. You shouldn't concern yourself with those things though - you should just run an uninstaller package and it will remove Keyboard Switch.

As with the installer package, you can also get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download and run the _KeyboardSwitchUninstaller-4.1.pkg_ file, and Keyboard Switch will be removed.

If you want to delete the app's configuration as well, then delete the hidden _.keyboard-switch_ folder in your home folder.

## Linux

### Installing the App

#### Prerequisites

There are several prerequisites for running the app on Linux:

* X11
* X Keyboard Extension (XKB) which is enabled by default
* X Test Extension - used to simulate pressing keys like _Ctrl+C_ and _Ctrl+V_ for you
* [xsel](https://github.com/kfish/xsel) - used to copy and paste text
* Bash - used to call xsel, but you don't need to have it as your default shell
* Freedesktop conventions - used to make the service app start when you log in, and to make the settings app appear in the list of your installed apps (not required for the app itself though)

Keyboard Switch doesn't support Wayland (even with XWayland apparently).

The most popular desktop systems (at least GNOME, KDE Plasma, Xfce, and LXQt) all adhere to the Freedesktop protocols, so the last prerequisite is automatically available, unless you're running a very unusual setup.

[Click here](https://github.com/TolikPylypchuk/KeyboardSwitch/issues/59) to see the list of Linux distributions on which the app was tested.

#### Using a Deb Package

If you're running a Debian-based distribution (e.g. Ubuntu or Mint), then you can install KeyboardSwitch using a deb package. You can get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

The package takes care of xsel and the X Test extension, so you don't need to install them yourself.

After installation, the settings app will be available in the list of installed apps, and the service app will be configured to run at login (it will be configured for all users though).

#### Using an RPM Package

If you're running a RHEL-based distribution (e.g. CentOS or Fedora), or SUSE, then you can install KeyboardSwitch using an RPM package. You can get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

The package takes care of xsel and the X Test extension, so you don't need to install them yourself.

{% hint style="info" %}
If you're using RHEL, CentOS, or Rocky Linux, then make sure that the EPEL repository is installed before installing KeyboardSwitch. Otherwise, it won't be able to install xsel.
{% endhint %}

{% hint style="info" %}
SUSE may complain that it cannot find libXtst when installing the app using the RPM package. This is because the package is targeted for RHEL and derivatives. You can ignore this warning and proceed with installation, but make sure that libXtst6 is installed before starting the app.
{% endhint %}

After installation, the settings app will be available in the list of installed apps, and the service app will be configured to run at login (it will be configured for all users though).

#### Using a Tar Archive

KeyboardSwitch is also available as a _tar.gz_ fie. You can get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). You can then extract it to anywhere you like (e.g. into the _/opt_ directory). The deployed app includes two scripts - _install.sh_ and _uninstall.sh_. _install.sh_ configures Freedesktop to start the KeyboardSwitch service when you log in, and to add the settings app to the list of installed apps. _uninstall.sh_ deletes this configuration.

Here are the steps required for installing the app on Debian, Ubuntu, Linux Mint, etc.:

```
sudo apt update
sudo apt install xsel libxtst6  # Install xsel and the X Test Extension
tar -xzf keyboard-switch-4.0-x64.tar.gz -C /opt
cd /opt/keyboard-switch
sudo chmod 755 install.sh uninstall.sh  # Let every user use the app
./install.sh
```

Here are the steps required for installing the app on RHEL, CentOS, Fedora, or Rocky Linux (use `yum` instead of `dnf` on older systems):

```
sudo dnf install epel-release  # Add the EPEL repository (not needed for Fedora)
sudo dnf install xsel          # Install xsel
sudo dnf install libXtst       # Install the X Test Extension
tar -xzf keyboard-switch-4.0-x64.tar.gz -C /opt
cd /opt/keyboard-switch
sudo chmod 755 install.sh uninstall.sh  # Let every user use the app
./install.sh
```

Here are the steps required for installing the app on SUSE (you may need to add the X11:XOrg repository to install libXtst6):

```
sudo zypper install xsel      # Install xsel
sudo zypper install libXtst6  # Install the X Test Extension
tar -xzf keyboard-switch-4.0-x64.tar.gz -C /opt
cd /opt/keyboard-switch
sudo chmod 755 install.sh uninstall.sh  # Let every user use the app
./install.sh
```

Here are the steps required for installing the app on Arch Linux:

```
sudo pacman -S xsel     # Install xsel
sudo pacman -S libxtst  # Install the X Test Extension
tar -xzf keyboard-switch-4.0-x64.tar.gz -C /opt
cd /opt/keyboard-switch
sudo chmod 755 install.sh uninstall.sh  # Let every user use the app
./install.sh
```

### Uninstalling the App

For the deb and RPM packages, it's simple - just uninstall the package. For the _tar.gz_ file, run the included _uninstall.sh_ script, and delete the app's directory. If you want to delete the app's configuration as well, then delete the _\~/.keyboard-switch_ directory.
