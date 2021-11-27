# Keyboard Switch

Version 4.0 (in progress)

Created by Tolik Pylypchuk

This application switches typed text as if it were typed with another keyboard layout.

Instead of manually retyping all that text that you have mistyped, you can switch it using this app. It will copy the
text, switch it, and paste it instantly. You just have to select the text and press the magic key combination.
That's it!

Keyboard Switch consists of two apps:

- Keyboard Switch Service - this app always runs in the background and does the switching when you press the magic key
combination

- Keyboard Switch Settings - this app is used to configure the service app

## Quick Start

You can find the latest release of the app [here](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

### Windows

You can use the installer to install the app.

After installation, the Keyboard Switch Settings app will start. It may take some time as the app will do some initial
setup. If it doesn't start for some reason, then find it in the list of your apps.

In the opened app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to
configure it further. But if you do, then all the info you'll ever need is provided in these docs.

To switch the forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_
twice instead.

### Linux

The app is available as a deb package (for Debian-based distributions, such as Ubuntu, Mint etc.), an RPM package (for
RHEL-based distributions, like CentOS or Fedora, as well as openSUSE), and a simple _tar.gz_ file. You'll have to get a
nightly build to get those files.

Bear in mind that KeyboardSwitch only works on X11 - it won't work on Wayland (even with XWayland apparently).

If you use the deb or RPM package, then simply install it either by double-clicking on it, or through the terminal. If
you use the _tar.gz_ file, then the set-up is not quite as quick, so you can read about it in the installation page.

After installing the app, open Keyboard Switch Settings - it should appear in the list of your apps.

In the opened app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to
configure it further. But if you do, then all the info you'll ever need is provided in these docs.

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_
twice instead.

## More Info

If you want to know more about the Keyboard Switch app and how to use it, check out the docs:
[https://docs.keyboardswitch.tolik.io](https://docs.keyboardswitch.tolik.io).

## Settings

The app is completely customizable. Before using this app you should make sure the characters are mapped correctly
according to the physical layout of keys on your keyboard.

You can auto-configure the character mappings

To configure the character mappings you have to enter every character you can think of (which can be entered using your
keyboard) into the text fields which correspond to layouts. For example, press the _Q_ key, then press the _W_ key, and
so on. Then press _Shift+Q_, then _Shift+W_ etc. to add uppercase letters. Remember that you should press the keys in
the same order for all layouts.

If you don't want to map a character into a certain layout, you can map it to the space character. The space character
is used as a _don't map this character_ instruction.

## Layouts

The app uses the list of your layouts in the same order as defined by the system. You can switch both forward and
backward through this list. The app also automatically changes your layouts, so you don't have to do it yourself. You
can disable this as well if you so wish. You cannot add a layout that's not present as one of the system's layouts. If
you add/remove a layout while the app is running, you'll have configure it in the settings and restart the service.

**Note:** This app is "Western-oriented". I developed it specifically to handle switching between Cyrillic and Latin
scripts. I don't know how it will work (if at all) with Eastern languages/scripts.

## Limitations

You can map every character only once per layout. Otherwise, it would be impossible to map characters deterministically.

Characters are always mapped one to one. You cannot map one character to several characters.

Dead keys are not supported (because of the previous limitation).

The space character cannot be mapped to other characters. This is not really a limitation, because the space character
is the same in every layout (at least that's the assumption).

## Supported Platforms

Version 3.0 works only on Windows 10/11. It can probably work on earlier versions of Windows as well, but I'm not
going to build or test it for them. [Version 4.0](https://github.com/TolikPylypchuk/KeyboardSwitch/milestone/4) on the
other hand is cross-platform - it already works on Linux (via X11). macOS support will come in a future version (most
probably 4.1).

Only the x64 architecture is supported. It would be ideal to also support ARM64, but not all dependencies of this app
currently support it. And even if they did, I don't have any devices with ARM64 to test the app there.

## Building from Source

You can build Keyboard Switch from source if you like. All projects (except the Windows installer) require .NET 6.

### Building the App Itself

Building the service app and the settings app is quite straightforward. You can simply use the `Build-Portable`
script located in the solution root. It will create the `KeyboardSwitch-Portable.zip` file in the `bin` folder
which you can then unpack into wherever you like.

Alternatively, you can build the projects using Visual Studio 2022 or later, or with the `dotnet` tool. Note however
that you have to build the whole solution. The startup project is `KeyboardSwitch.Settings` (the settings app), and
if you build it, it won't actually build the `KeyboardSwitch` project (ther service app), because the settings app
doesn't directly depend on the service app.

All projects (except the installer) are built into a shared `bin` folder located in the solution root. Again, this is
because the settings app needs the service app to be in the same folder, but the projects don't depend on each other.

It's better to use `dotnet publish` than simply using the raw build results. You can look into how the
`Build-Portable` script calls `dotnet publish`.

If you want to run the app of Linux through the `dotnet` tool, you have to always specify that `net6.0` is the target
framework since it uses `net6.0-windows` by default.

The installer project is excluded from the solution build sequence as it's not always needed.

### Building the Windows Installer

If you want to properly install the app, you can build the installer. Unlike the other projects, this one requires
.NET Framework 4.8. This is because it's built with [WixSharp](https://github.com/oleg-shilo/wixsharp), which in turn
is based on [the WiX toolset](https://wixtoolset.org), and as of version 3.11.2 WiX doesn't support .NET 6.

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
