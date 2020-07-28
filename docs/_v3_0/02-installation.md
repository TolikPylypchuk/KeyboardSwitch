---
title: Installation
permalink: /v3.0/installation
---

## Using a Windows Installer

You can get the latest version of the app from the
[releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the .msi file and run it
to install the app.

After installing the app, the installer will run the settings app. Currently it's not really optimized, so the startup
time is not perfect; it can take a couple seconds for the app to get started. On the first start up the settings app
will configure the service to run when you log into the system.

If multiple users use the PC, then bear in mind that the app configures itself to run on log-in only for the user which
installed the app. If you want it to run on log-in for other users, you have to configure it manually.

After the settings app starts up you should configure the character mappings (you can read more about it in the next
article). After you configured the character mappings and possibly some other preferences, start the app using the
'Start' button at the bottom of the window. That's it! Now you're ready to use the Keyboard Switch service.

## Uninstalling the App

You can uninstall the app using the Settings app just like you would uninstall any other app.

**Note:** The installer will not delete the registry entry which says that your app should start when you log in! If
you want to keep your registry clean, you have to disable it in the settings app before uninstalling it.
{: .notice--warning}

Upon uninstallation the installer will ask whether you want to delete the app's configuration as well. If you choose not
to delete the configuration, you can delete it later manually at any other time. It's stored under the user's local app
data folder.

**Note:** The installer will only delete the current user's configuration! If there are other users on the PC, they will
have to delete the configuration manually.
{: .notice--warning}

## Using the Portable Version

If you don't want to install the app (or can't), you can use the portable version of the app. It's literally the same as
the installable version; there are no differences. Again, go to the
[releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases) and dowload
KeyboardSwitch-Portable.zip. Extract the archive to anywhere you want and start KeyboardSwitchSettings.exe. You can
configure the app to run when you log in just like the installed version. The configuration for the portable version
is also stored under the user's local app data folder.
