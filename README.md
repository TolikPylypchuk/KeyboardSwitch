# Keyboard Switch

Version 4.0 (4.1 in progress). Created by Tolik Pylypchuk.

Keyboard Switch is an application which switches typed text as if it were typed with another keyboard layout.

Instead of manually retyping all that text that you have mistyped, you can switch it using this app. It will copy the
text, switch it, and paste it instantly. You just have to select the text and press the magic key combination.
That's it!

Keyboard Switch consists of two apps:

- Keyboard Switch Service - this app always runs in the background and does the switching when you press the magic key
combination

- Keyboard Switch Settings - this app is used to configure the service app

## Quick Start

You can find the latest release of the app [in the releases page](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

### Windows

Use the installer to install the app.

After installation, the Keyboard Switch Settings app will start. It may take some time as the app will do some initial
setup. If it doesn't start for some reason, then find it in the list of your apps.

In the opened app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_
twice instead.

### macOS

macOS version is not yet released, but you can get it from nigtly builds.

Use the _.pkg_ file to install the app. If your computer is running on an Intel CPU then you should get the _x86\_64_
package. If your computer is running on an Apple CPU (e.g. M1) then you should get the _arm64_ package.

After installing the app, open Keyboard Switch Settings - it should appear in the list of your apps.

Immediately upon opening the app a dialog window should appear which says that Keyboard Switch Service would like to
control this computer using accessibility features. The service app needs this to listen to the magic key combination
while running in the background, and without these permissions the app won't work.

Click the _Open System Preferences_ button on the dialog window. Unlock the settings and check the _Keyboard Switch
Service.app_ checkbox. Lock the settings, close System Preferences, and go back to the Keyboard Switch Settings app.

In the settings app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Option+Shift_
twice instead.

To uninstall Keyboard Switch you should run an uninstaller package. You can get it from a nightly build as well.

### Linux

The app is available as a deb package (for Debian-based distributions, such as Ubuntu, Mint etc.), an RPM package (for
RHEL-based distributions, like CentOS or Fedora, as well as SUSE), and a simple _tar.gz_ file.

Note that KeyboardSwitch only works on X11 - it won't work on Wayland (even with XWayland apparently).

If you use the deb or RPM package, then simply install it either by double-clicking on it, or through the terminal. If
you use the _tar.gz_ file, then the set-up is not quite as quick, so you can read about it in
[the installation page](https://docs.keyboardswitch.tolik.io/usage-guides/installation#linux).

Bear in mind that an RPM package may need
[additional setup](https://docs.keyboardswitch.tolik.io/usage-guides/installation#linux).

After installing the app, open Keyboard Switch Settings - it should appear in the list of your apps (if you desktop
environment groups your apps, then it will most probably appear under the _Utilities_ or _Accessories_ group).

In the opened app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_
twice instead.

## Supported Platforms

Version 4.0 works on Windows 10/11, and Linux via X11. macOS support will come in version 4.1. Wayland support may come
in a future version, but there are [several prerequisites](https://github.com/TolikPylypchuk/KeyboardSwitch/issues/54).
Versions up to 4.0 were Windows-only.

Only the x64 architecture is supported. Arm64 support will come in version 4.1, but for Windows and Linux it will be
experimental.

## More Info

If you want to know more about the Keyboard Switch app and how to use it, check out the docs:
[https://docs.keyboardswitch.tolik.io](https://docs.keyboardswitch.tolik.io).

## Changelog

See the [changlog file](https://github.com/TolikPylypchuk/KeyboardSwitch/blob/master/CHANGELOG.md) for the detailed list
of changes across versions.

## Icon

Icon made by [Smashicons](https://smashicons.com) from [www.flaticon.com](https://www.flaticon.com).
