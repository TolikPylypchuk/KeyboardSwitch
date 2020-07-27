---
title: Keyboard Switch 3.0
permalink: /v3.0/
---

Welcome to the Keyboard Switch 3.0 docs!

Keyboard Switch is an application which switches typed text as if it were typed with another keyboard layout.

If you ever write some text and then realise that you have written it using a wrong layout, you don't have to delete
it and start over anymore. Just select the text, press a magic key combination, and that's it!

I hope this app will make your life at least 1% easier :)

## How The App Works

The app is composed of two parts: the Keyboard Switch service and the Keyboard Switch Settings app. The service always
runs in the background (and starts when you log in to the system) and listens to key presses. When you press the magic
key combination, it reacts by copying the text, transforming it, and pasting it for you. The settings app is used for
configuring the service and for starting/stopping it manually.

Here are the basic steps you should take to switch the text:

- Type some text using an incorrect layout
- Realise your mistake
- Select the text (you can press Ctrl+A to select all text)
- Press the magic key combination (the default is pressing Ctrl+Shift twice)
- Profit

**Note:** The app simulates pressing Ctrl+C to copy and Ctrl+V to paste text. This is the default behaviour for most
(if not all) applications. But if you are working in an app which behaves differently on those key presses, this app
will not be able to instantly switch text. You can disable this behaviour, and copy/paste the text manually.
{: .notice--primary}

## Supported Platforms

This app works only on Windows 10. It can probably work on older versions of Windows as well, but I'm not going to
support them. Future versions of the app will be supported on other platforms, so if you don't use Windows then
stay tuned!

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
and restart the service.

**Note:** This app is "Western-oriented". I developed it specifically to handle switching between Cyrillic and Latin
scripts. I don't know how it will work (if at all) with Eastern languages/scripts.
{: .notice--primary}
