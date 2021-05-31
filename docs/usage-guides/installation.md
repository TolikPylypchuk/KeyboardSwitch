# Installation

{% hint style="warning" %}
Version 4.0 is not yet completed. The only way to get it is to build it yourself. It works on Windows and Linux, but it's not yet ready for macOS. The app itself and these docs may change at any moment without warning until version 4.0 is released.
{% endhint %}

## Windows

### Using a Windows Installer

~~You can get the latest version of the app from the~~ [~~releases page on GitHub~~](https://github.com/TolikPylypchuk/KeyboardSwitch/releases)~~. Download the _.msi_ file and run it to install the app.~~ You'll have to build the installer yourself if you want to install the app.

After installing the app, the installer will run the settings app. Currently it's not really optimized, so the startup time is not perfect; it can take a couple seconds for the app to get started. On the first start up the settings app will configure the service to run when you log into the system.

If multiple users use the PC, then bear in mind that the app configures itself to run on log-in only for the user that installed the app. Other users have to configure it manually.

When the settings app starts up you should configure the character mappings \(you can read more about it in the next article\). After you have configured the character mappings and possibly some other preferences, start the app using the _Start_ button at the bottom of the window. That's it! Now you're ready to use the Keyboard Switch service.

{% hint style="info" %}
**Note:** This app might mildly infuriate your anti-virus. This is because the app sets up a global keyboard hook, which means that it can react to all keyboard events in the entire system, and the anti-virus might think that it's a key logger. I can assure you, it's not - the app only reacts to the magic key combinations, and completely ignores all other keyboard events. If you don't trust me on this one, you can look through the app's code and build it yourself :\)
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

If you don't want to install the app \(or can't\), you can use the portable version of the app. It's literally the same as the installable version; there are no differences. ~~Again, go to the~~ [~~releases page on GitHub~~](https://github.com/TolikPylypchuk/KeyboardSwitch/releases) ~~and dowload _KeyboardSwitch-Portable.zip_.~~ If you run the _Build-Portable.ps1_ script, you'll have the _KeyboardSwitch-Portable.zip_ file. Extract the archive to anywhere you want and start _KeyboardSwitchSettings.exe_. You can configure the app to run when you log in just like the installed version. The configuration for the portable version is also stored under the user's local app data folder.

## Linux

### Installing the App

Currently, the Linux build is available only as a plain _.tar.gz_ file. Run the _buid.sh_ script to create _KeyboardSwitch.tar.gz_. You can then extract it to anywhere you like \(e.g. into the _/opt_ directory\). The  deployed app includes two scripts - _install.sh_ and _uninstall.sh_. _install.sh_ configures systemd to start the KeyboardSwitch service to start when you login, and _uninstall.sh_ deletes this configuration. The scripts edit files under _/etc/systemd/user_ so they will ask you for root privileges. If you don't use systemd, you'll have to figure out a different way of making sure the app starts when you log in.

There are several prerequisites for running the app on Linux:

* X11
* X Keyboard Extension \(XKB\) which is enabled by default
* X Test Extension \(you have to download it separately\) - used to simulate pressing keys like _Ctrl+C_ and _Ctrl+V_ for you
* [xsel](https://github.com/kfish/xsel) - used to copy and paste text
* Bash - used to call xsel, but you don't need to have it as your default shell
* systemd - used to make the service app start when you log in \(not required for the app itself though\)

Keyboard Switch doesn't support Wayland \(maybe through XWayland it will, but it's untested\).

Currently, the app was tested only on Ubuntu 20.04, but will be tested on many popular distributions.

Here are the steps required for installing the app on Ubuntu 20.04:

```text
sudo apt update
sudo apt install xsel libxtst-dev       # Install xsel and the X Test Extension
tar -xzf KeyboardSwitch.tar.gz -C /opt  # Extract the app
cd /opt/KeyboardSwitch
sudo chmod 777 install.sh uninstall.sh  # Let every user use the app
./install.sh
```

### Uninstalling the App

To uninstall the app, simply run the included _uninstall.sh_ script, and delete the app's directory. If you want to delete the app's configuration as well, then delete the _~/.KeyboardSwitch_ directory.

## macOS

The app is not supported on macOS yet, but implementing this is planned for version 4.0 as well.

