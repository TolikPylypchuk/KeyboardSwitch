---
layout: custom
title: Keyboard Switch
---

Keyboard Switch is an application which switches typed text as if it were typed with another keyboard layout.

Instead of manually retyping all that text that you have mistyped, you can switch it using this app. It will copy the
text, switch it, and paste it instantly. You just have to select the text and press the magic key combination.
That's it!

Keyboard Switch consists of two apps:

- Keyboard Switch Service - this app always runs in the background and does the switching when you press the magic key
combination

- Keyboard Switch Settings - this app is used to configure the service app

![App screen](/assets/images/app-screen.png)

# Quick Start

You can find the latest release of the app [in the releases page](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

## Windows

Download the _.msi_ file and run it to install the app.

After installation, the Keyboard Switch Settings app will start. It may take some time as the app will do some initial
setup. If it doesn't start for some reason, then find it in the list of your apps.

In the opened app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_
twice instead.

## macOS

Download the _.pkg_ file to install the app. If your computer is running on an Intel CPU then you should get the
_x86\_64_ package. If your computer is running on an Apple CPU (e.g. M1) then you should get the _arm64_ package.

After installing the app, open Keyboard Switch Settings - it should appear in the list of your apps.

Immediately upon opening the app a dialog window should appear which says that Keyboard Switch would like to control
this computer using accessibility features. The app needs this to listen to the magic key combination while running in
the background, and without these permissions it won't work.

Click the _Open System Preferences_ button on the dialog window. Unlock the settings and check the _Keyboard Switch_
checkbox. Lock the settings, close System Preferences, and go back to the Keyboard Switch Settings app.

In the settings app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Option+Shift_
twice instead.

To uninstall Keyboard Switch you should run an uninstaller package. You can get it from the releases page as well.

## Linux

The app is available as a deb package (for Debian-based distributions, such as Ubuntu, Mint etc.), an RPM package (for
RHEL-based distributions, like CentOS or Fedora, as well as SUSE), and a simple _tar.gz_ file.

Note that Keyboard Switch only works on X11 - it won't work on Wayland (even with XWayland).

If you use the deb or RPM package, then simply install it either by double-clicking on it, or through the terminal. If
you use the _tar.gz_ file, then the set-up is not quite as quick, so you can read about it in
[the installation page](https://docs.keyboardswitch.tolik.io/usage-guides/installation#linux).

Bear in mind that the RPM package may need
[additional setup](https://docs.keyboardswitch.tolik.io/usage-guides/installation#linux).

If your desktop environment is GNOME then you should restart it right after installation. If you're not sure which
desktop environment you're using then it's most probably GNOME since it's the default one on Ubuntu, Debian, CentOS,
Fedora and others (but not Linux Mint). Press _Alt+F2_, then type _r_ and press _Enter_. This will restart GNOME.

After installing the app, open Keyboard Switch Settings - it should appear in the list of your apps (if your desktop
environment groups your apps, then it will most probably appear under the _Utilities_ or _Accessories_ group).

In the opened app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_
twice instead.

# How the App Works

The app is composed of two parts: the Keyboard Switch service and the Keyboard Switch Settings app. The service always
runs in the background (and starts when you log into the system) and listens to key presses. When you press the magic
key combination, it reacts by copying the selected text, transforming it, and pasting it for you. The settings app is
used for configuring the service and for starting/stopping it manually.

Here are the basic steps you should take to switch the text:

- Type some text using an incorrect layout
- Realize your mistake
- Select the text (you can press _Ctrl+A_ to select all text, or _Command+A_ on macOS)
- Press the magic key combination (the default is pressing _Ctrl+Shift_ twice)
- Profit

The app uses the list of your layouts in the same order as defined by the system. You can switch both forward and
backward through this list. It will look at the current layout to determine how to switch text, so you shouldn't change
the layout before switching. The app also automatically changes the layout, so you don't have to do it yourself.

The app uses the clipboard to get the text to transform, and then puts the transformed text back into the clipboard.
The app will try to preserve the text that was in the clipboard before switching, and to restore it afterwards. It
doesn't guarantee that the text will be restored though. Also, any non-text data (e.g. a file or a picture) will not
be restored.

# Supported Platforms

Version 4.1 works on Windows 10/11, macOS 10.11+, and Linux via X11. Wayland support may come in a future version, but
there are [several prerequisites](https://github.com/TolikPylypchuk/KeyboardSwitch/issues/54). Versions up to 4.0 were
Windows-only.

The x64 and Arm64 architectures are both supported, but for Windows and Linux the latter is experimental since I don't
have any devices on which I can test the app.

# More Info

If you want to know more about the Keyboard Switch app and how to use it, check out the docs:
[https://docs.keyboardswitch.tolik.io](https://docs.keyboardswitch.tolik.io).

# Changelog

See the [changlog file](https://github.com/TolikPylypchuk/KeyboardSwitch/blob/master/CHANGELOG.md) for the detailed list
of changes across versions.
