# Keyboard Switch

Version 3.0

Created by Tolik Pylypchuk

This application switches typed text as if it were typed with another keyboard layout.

## Docs

If you want to know more about the Keyboard Switch app and how to use it, check out the docs:
[https://docs.keyboardswitch.tolik.io](https://docs.keyboardswitch.tolik.io).

## Switching

Instead of manually retyping all that text that you have mistyped, you can switch it using this app.
It will copy the text, switch it and paste it instantly. You just have to select the text and
press the key combination (the defaults are pressing Ctrl+Shift twice to switch forward and Alt+Ctrl+Shift twice to
switch backward).

## Settings

The app is completely customizable. Before using this app you should make sure the characters are mapped correctly
according to the physical layout of keys on your keyboard.

If you don't want to map a character into a certain layout, you can map it to the space character. The space character
is used as a _don't map this character_ instruction.

## Layouts

The app uses the list of your layouts in the same order as defined by the system. You can switch both forward
and backward through this list. The app also automatically changes your layouts, so you don't have to do it
yourself. You can disable this as well if you so wish. You cannot add a layout that's not present as one of
the system's layouts. If you add/remove a layout while the app is running, you'll have configure it in the settings
and restart the service.

**Note:** This app is "Western-oriented". I developed it specifically to handle switching between Cyrillic and Latin
scripts. I don't know how it will work (if at all) with Eastern languages/scripts.

## Limitations

You can map every character only once per layout. Otherwise, it would be impossible to map characters deterministically.

Characters are always mapped one to one. You cannot map one character to several characters.

Dead keys are not supported (because of the previous limitation).

The space character cannot be mapped to other characters. This is not really a limitation, because the space character
is the same in every layout (at least that's the assumption).

## Supported Platforms

Version 3.0 works only on Windows 10. It can probably work on earlier versions of Windows as well, but I'm not
going to build or test it for them. [Version 4.0](https://github.com/TolikPylypchuk/KeyboardSwitch/milestone/4) on the
other hand will be cross-platform - I'm planning on making it work on macOS and on Linux (via X11), though the details
of this plan may change when I'll start looking into all that. The app will most probably use
[libuiohook](https://github.com/kwhat/libuiohook) for the cross-platform keyboard hook.

## Building from Source

You can build Keyboard Switch from source if you like. All projects (except the Windows installer) require .NET Core 3.1
or later.

### Building the App Itself

Building the service app and the settings app is quite straightforward. You can simply use the `Build-Portable`
script located in the solution root. It will create the `KeyboardSwitch-Portable.zip` file in the `bin` folder
which you can then unpack into wherever you like.

Alternatively, you can build the projects using Visual Studio 2019 or later, or with the `dotnet` tool. Note however
that you have to build the whole solution. The startup project is `KeyboardSwitch.Settings` (the settings app), and if
you build it, it won't actually build the `KeyboardSwitch` project (ther service app), because the settings app doesn't
directly depend on the service app.

All projects (except the installer) are built into a shared `bin` folder located in the solution root. Again, this is
because the settings app needs the service app to be in the same folder, but the projects don't depend on each other.

It's better to use `dotnet publish` than simply using the raw build results. You can look into how the `Build-Portable`
script calls `dotnet publish`.

The installer project is excluded from the solution build sequence as it's not always needed.

### Building the Windows Installer

If you want to properly install the app, you can build the installer. Unlike the other projects, this one requires
.NET Framework 4.8. This is because it's built with [WixSharp](https://github.com/oleg-shilo/wixsharp), which in turn
is based on [the WiX toolset](https://wixtoolset.org), and as of version 3.11.2 WiX doesn't support .NET Core or .NET 5.

Simply run the build, and it will generate the MSI installer in the project's `bin` folder. Before the build, it calls
`dotnet publish` to use its output.

After installation the settings app is started, and it automatically configures the service to run at system startup.
Currently the app's startup time is quite long, so it may appear after a couple of seconds.

## Changes

### What's new in version 3.0

- A completely rewritten app based on .NET Core and working as a truly UI-less app
instead of an app with a hidden window
- The settings are now a separate app written with Avalonia instead of WPF
- Tray icon was removed
- Added the ability to switch between multiple layouts of the same language (e.g. QWERTY and Dvorak for English)
- Instant switching mode is not experimental anymore and enabled by default
- Added the ability to auto-configure character mappings
- Added checking for updates

### What's new in version 2.1

- Minor bug fixes

### What's new in version 2.0

- Brand new installer for the app
- Instant switching mode
- Updated readme
- Updated configuration
- Minor UI updates
- Minor bug fixes

### What's new in version 1.1

- Minor bug fixes

## Icon

Icon made by [Smashicons](https://smashicons.com/) from [www.flaticon.com](https://www.flaticon.com/).
