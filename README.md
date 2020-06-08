# Keyboard Switch

Version 3.0 (Work in progress)

Created by Tolik Pylypchuk

This application switches typed text as if it were typed with another keyboard layout.

## Switching

Instead of manually retyping all that text that you have mistyped, you can switch it using this app.
It will copy the text, switch it and paste it instantly. You just have to select the text and
press the key combination (the defaults are Ctrl+Shift+X to switch forward and Ctrl+Shift+Z to switch backward).

**Note:** A few things to consider:

 - The app simulates pressing Ctrl+C to copy and Ctrl+V to paste text. This is the default behaviour for most
(if not all) applications. But if you are working in an app which behaves differently on those key presses, this app
will not be able to instantly switch text. You can disable this behaviour, and copy/paste the text manually.

 - Ctrl+Shift work best as the modifier keys for instant switching because pressing Alt will change the focus
to some kind of a menu in most apps.

## Settings

The app is completely customizable. Common layouts for a couple languages are built-in. Before using this app
you should make sure the characters are mapped correctly according to the physical layout of keys on your keyboard.

If you don't want to map a character into a certain layout, you can map it to the space character. The space character
is used as a _don't map this character_ instruction.

## Limitations

You can map every character only once per language. Otherwise, it would be impossible to map characters
deterministically.

Characters are always mapped one to one. You cannot map one character to several characters.

Dead keys are not supported (becuase of the previous limitation).

The space character cannot be mapped to other characters. This is not really a limitation, because the space character
is the same in every layout (at least that's the assumption).

## Languages

The app uses the list of your layouts in the same order as defined by the system. You can switch both forward
and backward through this list. The app also automatically changes your layouts, so you don't have to do it
yourself. You can disable this as well if you so wish. You cannot add a layout that's not present as one of
the system's layouts. If you add/remove a layout while the app is running, you'll have configure it in the settings
to restart the service.

This app is "Western-oriented". I developed it specifically to handle switching between Cyrillic and Latin scripts.
I don't know how it will work (if at all) with Eastern languages/scripts.

I hope this app will make your life at least 1% easier :)

## Changes

### What's new in version 3.0 (Work in progress)

  - A completely rewritten app based on .NET Core and working as a truly UI-less app
instead of an app with a hidden window
  - The settings are now a separate app written with Avalonia instead of WPF
  - Tray icon was removed
  - Added the ability to switch between multiple layouts of the same language (e.g. QWERTY and Dvorak for English)
  - Instant switching mode is not experimental anymore and enabled by default
  - Added the ability to auto-configure character mappings

### What's new in version 2.1:

- Minor bug fixes

### What's new in version 2.0:

- Brand new installer for the app
- Instant switching mode
- Updated readme
- Updated configuration
- Minor UI updates
- Minor bug fixes

### What's new in version 1.1:

- Minor bug fixes

## Icon

Icon made by [Smashicons](https://smashicons.com/) from [www.flaticon.com](https://www.flaticon.com/).
