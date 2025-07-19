# Keyboard Switch

Welcome to the Keyboard Switch docs!

Keyboard Switch is an application which switches typed text as if it were typed with another keyboard layout.

If you ever write some text and then realize that you have written it using a wrong layout, you don't have to delete it and start over anymore. Just select the text, press a magic key combination, and that's it!

I hope this app will make your life at least 1% easier :)

## Quick Start

### Windows

You can get the latest version of the app from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the _.msi_ file and run it to install the app. If your computer runs on the x64 architecture, then you should get the _x64_ installer. If your computer runs on the Arm architecture, then you should get the _arm64_ installer. If you're not sure, then you most likely need the _x64_ installer.

After installation, the Keyboard Switch Settings app will start. It may take some time as the app will do some initial setup. If it doesn't start for some reason, then find it in the list of your apps.

In the opened app, press _Start_.

That's it! You're ready to use Keyboard Switch in its basic configuration. There's a big chance you won't need to configure it further. But if you do, then all the info you'll ever need is provided in these docs.

To switch text forward, select the text and press <kbd>Ctrl</kbd>+<kbd>Shift</kbd> twice. To switch it backward, press <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Shift</kbd> twice instead.

### macOS

You can get the latest version of the app from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the _.pkg_ file and run it to install the app. If your computer runs on an Apple CPU (e.g. M1) then you should get the _arm64_ package. If your computer runs on an Intel CPU then you should get the _x86\_64_ package.

After installing the app, open Keyboard Switch Settings – it should appear in the list of your apps.

Immediately upon opening the app, a dialog window should appear which says that Keyboard Switch would like to control this computer using accessibility features. The app needs this to listen to the magic key combination while running in the background, and without these permissions it won't work.

Click the _Open System Preferences_ button on the dialog window. Enable the _Keyboard Switch_ item. Close System Preferences and go back to the Keyboard Switch Settings app.

In the settings app, press _Start_.

That's it! You're ready to use Keyboard Switch in its basic configuration. There's a big chance you won't need to configure it further. But if you do, then all the info you'll ever need is provided in these docs.

To switch text forward, select the text and press <kbd>Ctrl</kbd>+<kbd>Shift</kbd> twice. To switch it backward, press <kbd>Ctrl</kbd>+<kbd>Option</kbd>+<kbd>Shift</kbd> twice instead.

To uninstall Keyboard Switch you should run an uninstaller package. You can also get it from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

### Linux

The app is available as a deb package (for Debian-based distributions, such as Ubuntu, Mint etc.), an RPM package (for RHEL-based distributions, like Rocky Linux or Fedora, as well as SUSE), and a simple _tar.gz_ file. You can get these files from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

{% hint style="info" %}
If you're using an RPM package, there are a couple things to note before installing the app (which you can find in the [installation](usage-guides/installation.md#installing-the-app) article).
{% endhint %}

Bear in mind that Keyboard Switch only works on X11 – it won't work on Wayland (even with XWayland).

If you use the deb or RPM package, then simply install it either by double-clicking on it, or through the terminal. If you use the _tar.gz_ file, then the set-up is not quite as quick, so you can read about it in the [installation page](usage-guides/installation.md#installing-the-app).

After installing the app, open Keyboard Switch Settings – it should appear in the list of your apps.

{% hint style="warning" %}
If your desktop environment is GNOME, then you should restart it right after opening Keyboard Switch Settings. If you're not sure which desktop environment you're using then it's most probably GNOME since it's the default one on Ubuntu, Debian, Rocky, Fedora and others (but not Linux Mint). Press <kbd>Alt</kbd>+<kbd>F2</kbd>, then type <kbd>r</kbd> and press <kbd>Enter</kbd>. This will restart GNOME.
{% endhint %}

In the opened app, press _Start_.

That's it! You're ready to use Keyboard Switch in its basic configuration. There's a big chance you won't need to configure it further. But if you do, then all the info you'll ever need is provided in these docs.

To switch text forward, select the text and press <kbd>Ctrl</kbd>+<kbd>Shift</kbd> twice. To switch it backward, press <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Shift</kbd> twice instead.

## How the App Works

The app is composed of two parts: the Keyboard Switch service application and the Keyboard Switch Settings application. The service always runs in the background (and starts when you log into the system) and listens to key presses. When you press the magic key combination, it reacts by copying the selected text, transforming it, and pasting it for you. The settings app is used for configuring the service and for starting/stopping it manually.

Here are the basic steps you should take to switch the text:

* Type some text using an incorrect layout
* Realize your mistake
* Select the text (you can press <kbd>Ctrl</kbd>+`A` to select all text, or `Command`+<kbd>A</kbd> on macOS)
* Press the magic key combination (the default is pressing <kbd>Ctrl</kbd>+<kbd>Shift</kbd> twice)
* Profit

The app uses the list of your layouts in the same order as defined by the system. You can switch both forward and backward through this list. It will look at the current layout to determine how to switch text, so you shouldn't change the layout before switching. The app also automatically changes the layout, so you don't have to do it yourself.

The app uses the clipboard to get the text to transform and then puts the transformed text back into the clipboard. The app will try to preserve the text that was in the clipboard before switching and to restore it afterwards. It doesn't guarantee that the text will be restored though. Also, any non-text data (e.g., a file or a picture) will not be restored.

{% hint style="info" %}
**Note:** The app simulates pressing <kbd>Ctrl</kbd>+<kbd>C</kbd> (or <kbd>Command</kbd>+<kbd>C</kbd> on macOS) to copy and <kbd>Ctrl</kbd>+<kbd>V</kbd> (or <kbd>Command</kbd>+<kbd>V</kbd> on macOS) to paste text. This is the default behavior for most applications. But if you are working in an app which behaves differently on those key presses (e.g., a terminal), this app will not be able to instantly switch text. You can disable this behavior and copy/paste the text manually.
{% endhint %}

## Supported Platforms

Keyboard Switch currently works on Windows 10/11 (version 1607 or later), macOS 10.15 or later, and Linux via X11. As for Linux, it needs several prerequisites to run (more on that on [the installation page](usage-guides/installation.md#linux)).

The x64 and Arm64 architectures are both supported.
