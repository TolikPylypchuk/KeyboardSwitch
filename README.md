# Keyboard Switch

Keyboard Switch is an application which switches typed text as if it were typed with another keyboard layout.

Instead of manually retyping all that text that you have mistyped, you can switch it using this app. It will copy the
text, switch it, and paste it instantly. You just have to select the text and press the magic key combination.
That's it!

Keyboard Switch consists of two apps:

- Keyboard Switch Service – this app always runs in the background and does the switching when you press the magic key
combination

- Keyboard Switch Settings – this app is used to configure the service app

## Quick Start

You can get the latest release of the app [in the releases page](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

### Windows

Download the _.msi_ file and run it to install the app. If your computer runs on the x64 architecture, then you should
get the _x64_ installer. If your computer runs on the Arm architecture, then you should get the _arm64_ installer. If
you're not sure, then you most likely need the _x64_ installer.

After installation, the Keyboard Switch Settings app will start. It may take some time as the app will do some initial
setup. If it doesn't start for some reason, then find it in the list of your apps.

In the opened settings app, press _Start_.

That's it! You're ready to use Keyboard Switch in its basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press <kbd>Ctrl</kbd>+<kbd>Shift</kbd> twice. To switch it backward, press
<kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Shift</kbd> twice instead.

### macOS

Download the _.pkg_ file to install the app. If your computer runs on an Intel CPU then you should get the _x86\_64_
package. If your computer runs on an Apple CPU (e.g. M1) then you should get the _arm64_ package.

After installing the app, open Keyboard Switch Settings – it should appear in the list of your apps.

Immediately upon opening the app, a dialog window should appear which says that Keyboard Switch would like to control
this computer using accessibility features. The app needs this to listen to the magic key combination while running in
the background, and without these permissions it won't work.

Click the _Open System Preferences_ button on the dialog window. Enable the _Keyboard Switch_ item. Close System
Preferences and go back to the Keyboard Switch Settings app.

In the settings app, press _Start_.

That's it! You're ready to use Keyboard Switch in its basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press <kbd>Ctrl</kbd>+<kbd>Shift</kbd> twice. To switch it backward, press
<kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Shift</kbd> twice instead.

To uninstall Keyboard Switch you should run an uninstaller package. You can get it from the releases page as well.

### Linux

The app is available as a deb package (for Debian-based distributions, such as Ubuntu, Mint etc.), an RPM package (for
RHEL-based distributions, like Rocky Linux or Fedora, as well as SUSE), and a simple _tar.gz_ file.

Note that Keyboard Switch only works on X11 – it won't work on Wayland (even with XWayland).

If you use the deb or RPM package, then simply install it either by double-clicking on it, or through the terminal. If
you use the _tar.gz_ file, then the set-up is not quite as quick, so you can read about it in
[the installation page](https://docs.keyboardswitch.tolik.io/usage-guides/installation#linux).

Bear in mind that the RPM package may need
[additional setup](https://docs.keyboardswitch.tolik.io/usage-guides/installation#linux).

After installing the app, open Keyboard Switch Settings – it should appear in the list of your apps (if your desktop
environment groups your apps, then it will most probably appear under the _Utilities_ or _Accessories_ group).

If your desktop environment is GNOME then you should restart it right after opening Keyboard Switch Settings. If you're
not sure which desktop environment you're using then it's most probably GNOME since it's the default one on Ubuntu,
Debian, Rocky Linux, Fedora and others (but not Linux Mint). Press <kbd>Alt</kbd>+<kbd>F2</kbd>, then type <kbd>r</kbd>
and press <kbd>Enter</kbd>. This will restart GNOME.

In the opened settings app, press _Start_.

That's it! You're ready to use Keyboard Switch in its basic configuration. There's a big chance you won't need to
configure it further. But if you do, the you can read more [in the docs](https://docs.keyboardswitch.tolik.io).

To switch text forward, select the text, and press <kbd>Ctrl</kbd>+<kbd>Shift</kbd> twice. To switch it backward, press
<kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Shift</kbd> twice instead.

## How the App Works

The app is composed of two parts: the Keyboard Switch service and the Keyboard Switch Settings app. The service always
runs in the background (and starts when you log into the system) and listens to key presses. When you press the magic
key combination, it reacts by copying the selected text, transforming it, and pasting it for you. The settings app is
used for configuring the service and for starting/stopping it manually.

Here are the basic steps you should take to switch the text:

- Type some text using an incorrect layout
- Realize your mistake
- Select the text (you can press <kbd>Ctrl</kbd>+<kbd>A</kbd> to select all text, or <kbd>Command</kbd>+<kbd>A</kbd> on
macOS)
- Press the magic key combination (the default is pressing <kbd>Ctrl</kbd>+<kbd>Shift</kbd> twice)
- Profit

The app uses the list of your layouts in the same order as defined by the system. You can switch both forward and
backward through this list. It will look at the current layout to determine how to switch text, so you shouldn't change
the layout before switching. The app also automatically changes the layout, so you don't have to do it yourself.

The app uses the clipboard to get the text to transform, and then puts the transformed text back into the clipboard.
The app will try to preserve the text that was in the clipboard before switching, and to restore it afterwards. It
doesn't guarantee that the text will be restored though. Also, any non-text data (e.g., a file or a picture) will not
be restored.

## Supported Platforms

Keyboard Switch works on Windows 10/11 (version 1607 or later), macOS 10.15 or later, and Linux via X11. Wayland support
may come in a future version, but there are
[several prerequisites](https://github.com/TolikPylypchuk/KeyboardSwitch/issues/54).

The x64 and Arm64 architectures are both supported.

## More Info

If you want to know more about the Keyboard Switch app and how to use it, check out the docs:
[https://docs.keyboardswitch.tolik.io](https://docs.keyboardswitch.tolik.io).

## Changelog

See the [changlog file](https://github.com/TolikPylypchuk/KeyboardSwitch/blob/main/CHANGELOG.md) for the detailed list
of changes across versions.

## Icon

Icon made by [Smashicons](https://smashicons.com) from [www.flaticon.com](https://www.flaticon.com).
