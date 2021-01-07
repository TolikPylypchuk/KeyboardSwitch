---
layout: custom
title: Keyboard Switch
---

Keyboard Switch is an application which switches typed text as if it were typed with another keyboard layout.

Instead of manually retyping all that text that you have mistyped, you can switch it using this app. It will copy the
text, switch it and paste it instantly. You just have to select the text and press the key combination (the defaults
are pressing Ctrl+Shift twice to switch forward and Alt+Ctrl+Shift twice to switch backward).

# Settings

The app is completely customizable. Before using this app you should make sure the characters are mapped correctly
according to the physical layout of keys on your keyboard.

If you don't want to map a character into a certain layout, you can map it to the space character. The space character
is used as a don't map this character instruction.

![App screen](/assets/images/app-screen.png)

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
build or test it for them. Version 4.0 on the other hand will be cross-platform - I'm planning on making it work on
macOS and on Linux (via X11), though the details of this plan may change when I'll start looking into all that. The app
will most probably use libuiohook for the cross-platform keyboard hook.

# Docs

The docs for this app are available [here](https://docs.keyboardswitch.tolik.io) and contain extensive info on how to use
the app.
