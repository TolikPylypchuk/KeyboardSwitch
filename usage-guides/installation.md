# Installation

## Windows

### Using a Windows Installer

You can get the latest version of the app from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the _.msi_ file and run it to install the app.

After installing the app, the installer will run the settings app. Currently it's not really optimized, so the startup time is not perfect; it can take a couple seconds for the app to get started. On the first start-up the settings app will configure the service to run when you log into the system.

If multiple users use the PC, then bear in mind that the app configures itself to run on log-in only for the user that installed the app. Other users have to configure it manually.

When the settings app starts up you should configure the character mappings (you can read more about it in the next article). After you have configured the character mappings and possibly some other preferences, start the app using the _Start_ button at the bottom of the window. That's it! Now you're ready to use the Keyboard Switch service.

{% hint style="info" %}
**Note:** This app might mildly infuriate your anti-virus. This is because the app sets up a global keyboard hook, which means that it can react to all keyboard events in the entire system, and the anti-virus might think that it's a key logger. I can assure you, it's not - the app only reacts to the magic key combinations, and completely ignores all other keyboard events. If you don't trust me on this one, you can look through the app's code and build it yourself :)
{% endhint %}

### Uninstalling the App

You can uninstall the app using the Settings app just like you would uninstall any other app.

{% hint style="warning" %}
**Note:** The installer will not delete the registry entry which says that your app should start when you log in! If you want to keep your registry clean, you have to disable it in the settings app before uninstalling it.
{% endhint %}

Upon uninstallation the installer will ask whether you want to delete the app's configuration as well. If you choose not to delete the configuration, you can delete it later manually at any other time. It's stored under the user's local app data folder.

{% hint style="warning" %}
**Note:** The installer will only delete the current user's configuration! If there are other users on the PC, they will have to delete the configuration manually.
{% endhint %}

### Using the Portable Version

If you don't want to install the app (or can't), you can use the portable version of the app. It's literally the same as the installable version; there are no differences. Again, go to the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases) and download _KeyboardSwitch-Portable.zip_. Extract the archive to anywhere you want and start _KeyboardSwitchSettings.exe_. You can configure the app to run when you log in just like the installed version. The configuration for the portable version is also stored under the user's local app data folder.

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