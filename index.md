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

# Installing the App

You can find the latest release of the app [here](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

## Windows

You can use the installer to install the app, or you can download the portable version of the app. There's not much
difference between them.

## Linux

Work on the Linux version is currently in progress, but you can build from source, and it will most probably work. You
can find more info on GitHub.

# Quick Start

Firstly, start the settings app. If you use the installer to get the app, then the settings app will start on its own
right after installation.

![App screen](/assets/images/app-screen.png)

You need to let the app know how to map characters of your layouts. You can do that yourself, or you can let the app
auto-configure it.

Next, start the service app. To do that, press the 'Start' button at the bottom of the settings app's window.

Now that you've configured Keyboard Switch, you don't really need to open the settings app ever again (unless you
change system settings of your keyboard or you want to chagne the app's settings).

To switch the text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_
twice instead.

# Settings

The app is completely customizable. Before using this app you should make sure the characters are mapped correctly
according to the physical layout of keys on your keyboard.

To configure the character mappings you have to enter every character you can think of (which can be entered using
your keyboard) into the text fields which correspond to layouts. For example, press the Q key, then press the W key,
and so on. Then press Shift+Q, then Shift+W etc. to add uppercase letters. Remember that you should press the keys in
the same order for all layouts.

If you don't want to map a character into a certain layout, you can map it to the space character. The space character
is used as a don't map this character instruction.

# Layouts

The app uses the list of your layouts in the same order as defined by the system. You can switch both forward and
backward through this list. The app also automatically changes your layouts, so you don't have to do it yourself. You
can disable this as well if you so wish. You cannot add a layout that's not present as one of the system's layouts. If
you add/remove a layout while the app is running, you'll have configure it in the settings and restart the service.

**Note:** This app is "Western-oriented". I developed it specifically to handle switching between Cyrillic and Latin
scripts. I don't know how it will work (if at all) with Eastern languages/scripts.

# Limitations

You can map every character only once per layout. Otherwise, it would be impossible to map characters deterministically.

Characters are always mapped one to one. You cannot map one character to several characters.

Dead keys are not supported (because of the previous limitation).

The space character cannot be mapped to other characters. This is not really a limitation, because the space character
is the same in every layout (at least that's the assumption).

# Supported Platforms

Version 3.0 works only on Windows 10. It can probably work on earlier versions of Windows as well, but I'm not going to
build or test it for them. Version 4.0 on the other hand will be cross-platform - it already works on Linux (via X11)
and I'm planning on making it work on macOS as well.

Only the x64 architecture is supported. It would be ideal to also support arm64, but not all dependencies of this app
currently support it. And even if they did, I don't have any devices with arm64 to test the app there.

# Docs

The docs for this app are available [here](https://docs.keyboardswitch.tolik.io) and contain extensive info on how to use
the app.
