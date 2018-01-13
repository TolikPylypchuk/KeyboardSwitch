# Keyboard Layout Switch

Version 2.0

Created by Tolik Pylypchuk

This application switches typed text
as if it were typed with another keyboard layout.

## Installation

This app can be either installed or used as a portable app.
The only difference is that the installed version gets
a shortcut in the Start Menu and will be started when
you log into the system.

## Default Switching

To switch text, all you have to do is select the mistyped text,
cut (or copy) it to the clipboard, press the key combination
of your choice (the defaults are Ctrl+Alt+X to switch forward
and Ctrl+Alt+Z to switch backward) and then just paste
the switched text.

## Instant Switching

In addition to the default switching mode this app also
offers the instant mode.

**Warning!**
Instant switching mode is less stable than the default mode.
Sometimes it doesn't switch properly. Use it with caution.

Instead of copying and pasting the text manually, you can
use the instant switching mode. It will copy the text, switch it
and paste it in one go. You just have to select the text and
press the key combination (the defaults are Ctrl+Shift+X
to switch forward and Ctrl+Shift+Z to switch backward).

**Note:** A few things to consider:

- The app simulates pressing Ctrl+C to copy
and Ctrl+V to paste text. This is the default
behaviour for most (if not all) applications.
But if you are working in an app which behaves
differently on those key presses, this app will
not be able to switch text.

- Ctrl+Shift work best as the modifier keys for instant
switching because pressing Alt will change the focus
to some kind of a menu in most apps.

## Settings

The app is completely customisable:
you can set character mappings, add new ones, delete them,
and even move them around.

To quickly change mappings, you can use the TAB key. It will move
the input focus to the next item in the mapping table.
In addition, it will change the input language so you don't have
to do it yourself.

By default scrolling in the settings window is horizontal.
If you want to scroll vertically, press Ctrl while you scroll.
Touchpad horizontal scrolling is not supported.

The app runs in the background, so if you close
the settings window, it will not shut down.
It uses a tray icon, so you can see that it's there.

## Languages

The app uses the list of your input languages in the same order
as defined by the system. You can switch both forward
and backward through this list. The app also automaticly
changes your input language, so you don't have to do it yourself.
You cannot add a language that's not present as one of
the system's input languages.
If you add/remove an input language while the app is running,
you'll have to restart it.

**Note:** The app uses a single layout for a language.
e.g. if you are using both English QWERTY and English DVORAK,
you can only set one of them for the app to switch.

## Configuration

The mappings that this app creates are stored in the
file called mappings.dat. The file itself is stored in the
local app data folder.

There is another mappings.dat file - the one stored with the
app. This file contains the default mappings. They are used
when the app is started for the first time for a user.

## Changes

### What's new in version 2.0:

- Brand new installer for the app.
- Instant switching mode.
- Updated readme.
- Updated configuration
- Minor UI updates.
- Minor bug fixes.

### What's new in version 1.1:

- Minor bug fixes.

I hope this app will make your life at least 1% easier :)
