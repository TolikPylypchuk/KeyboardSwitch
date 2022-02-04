# Technical Stuff

Keyboard Switch is written using [C#](https://github.com/dotnet/csharplang) and [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0). Following is the list of technologies used in this app and some other technical aspects.

## UI

The Keyboard Switch Settings app is written using [Avalonia](https://avaloniaui.net), a cross-platform UI framework for .NET. I chose it because unlike other UI frameworks for .NET, it's cross-platform, and I had to do literally nothing for it to work on macOS and Linux as well. The style is provided by [FluentAvalonia](https://github.com/amwx/FluentAvalonia).

Previous versions of the app used .NET Framework and WPF. Back then I didn't expect to have it working on different platforms.

Previous versions of the app also included a tray icon so you could see that the app is running. I removed it in version 3.0 as I don't see much value in keeping it and cluttering your tray.

## Core Logic

### Windows

The service app is just an app which runs in the background. I could have made it into a Windows service, but decided against it when I read somewhere that Windows services cannot set up keyboard hooks. I didn't actually verify this info, so I'm not sure it's correct, but the background app approach works just fine, so it's likely to stay this way.

The service app calls various native functions from the Windows' _user32.dll_. It uses [Vanara](https://github.com/dahall/Vanara) to call them.

In the previous versions (up to 3.0) the service and the settings app were not separated. It was just an app running with a hidden window which showed up when you opened it. Having a hidden window always loaded is not that great of an idea, even though it used very little RAM.

### macOS

On macOS the service app runs as a `launchd` service. `launchd` provides the ability to start at login. The settings app starts the service app through `launchd` as well. macOS 10.11+ is required since it contained some major changes in `launchd`.

The service app uses multiple native macOS frameworks:

* CoreFoundation - for low-level string, array, and pointer manipulation.
* CarbonCore (part of CoreFoundation) - for translating key codes with layout info into Unicode characters.
* CoreGraphics - for simulating keyboard events.
* HIToolbox - for working with keyboard layouts.
* AppKit - for working with the clipboard.

It uses functions and constants from these frameworks using P/Invoke directly, except for AppKit - that one is called by the [TextCopy](https://github.com/CopyText/TextCopy) library which the app uses for copying and pasting text.

### Linux

At first I decided to run the app as a systemd service, but it proved not to provide much value, so I've since reverted this decision.

The service app uses X11, especially the X Keyboard Extension, and the X Test Extension. Currently it doesn't work on Wayland, even through XWayland. It calls various native Xlib functions using P/Invoke directly.

The app also uses Bash and [xsel](https://github.com/kfish/xsel), but those are not used directly - rather they are used by [TextCopy](https://github.com/CopyText/TextCopy).

If the Linux desktop environment is GNOME then the app switches layouts a little differently than in other DEs. GNOME doesn't let apps switch layouts using X11 directly - it will immediately switch it back. Instead, the app installers also add a small GNOME Shell extension (which is simply called Switch Layout) and the app switches layouts through it. GNOME should be made aware of this extension after installation, hence you should restart it after installing the app. If you don't then the app will still work, but won't be able to switch layouts until you log out or reboot.

### The Settings App

The core logic of the settings app is implemented using [ReactiveUI](https://www.reactiveui.net) as the MVVM framework. I must say that it’s a real pleasure to use this framework. Also, [Splat](https://github.com/reactiveui/splat) is used for service location.

## App Structure

Keyboard Switch's files are located in the same directory on Windows and Linux - and there are a bunch of them since these are two self-contained .NET applications. The apps could be published as single-file applications, but they share quite a bit of libraries, so these libraries would be duplicated for both apps.

On macOS the two applications are located in separate directories - this is done because on macOS every application should be contained inside a bundle. The apps are published as single-file apps for that platform as the shared libraries would have to be duplicated either way.

## The Global Keyboard Hook

The app uses [libuiohook](https://github.com/kwhat/libuiohook) to create a global keyboard hook. The library is cross-platform and supports almost all popular OSes and architectures. Since this library is native and it's non-trivial to build, I've developed a .NET wrapper for it, [SharpHook](https://github.com/TolikPylypchuk/SharpHook), so that I don't have to deal with a native library in this project.

Previously the app used a Windows keyboard hook directly, but that doesn't work on other systems.

## Preferences

Preferences are stored in the user's local app data folder (under the _KeyboardSwitch_ directory) on Windows. I didn't put it into the normal app data folder because that one is shared if you use the same Windows account on multiple machines, and that kind of defeats the purpose of this app being configured for each machine individually.

On macOS and Linux the settings are stored in the _\~/.keyboard-switch_ directory.

This app uses [Akavache](https://github.com/reactiveui/Akavache) as the engine for storing settings.

## Logging

This app uses a plain-text log file to log the stuff it’s doing and errors it encounters along the way. [Serilog](https://serilog.net) is used as the logging library. The log file is stored in the same folder as the preferences and is rolled over after reaching 10 MB. If you really want to (and know how), you can change the logging configuration in the _appsettings.json_ file. On Windows and Linux this file is located in the same folder as the app itself. On macOS this file is located in the bundle's resources folder, and you probably shouldn't change it as it may break the bundle's signature (though I'm not sure about that).

## Tests

There are none. Honestly, I don't even know how to approach testing, because the core logic of the app is essentially creating system-wide side effects, so don't think it's really testable. As for the settings app, well, I could test it, but that would take a lot of time, and the app is pretty simple, so I don't plan on doing it in the nearest future.

## Docs

The docs are built and hosted on [GitBook](https://www.gitbook.com).

Previously these articles were built using [Jekyll](https://jekyllrb.com) and hosted on [GitHub Pages](https://pages.github.com), and used the [Minimal Mistakes theme](https://mmistakes.github.io/minimal-mistakes).

## Building from Source

You can build Keyboard Switch from source if you like. All projects (except the Windows installer) require .NET 6.

### The Projects

The app consists of 8 projects:

* KeyboardSwitch: The Keyboard Switch service itself
* KeyboardSwitch.Core: Core functionality and interfaces for the projects
* KeyboardSwitch.Settings: The Keyboard Switch Settings app
* KeyboardSwitch.Settings.Core: The core logic of the settings app
* KeyboardSwitch.Windows: The implementation of the core functionality for the Windows platform
* KeyboardSwitch.MacOS: The implementation of the core functionality for the macOS platform
* KeyboardSwitch.Linux: The implementation of the core functionality for the Linux platform
* KeyboardSwitch.Windows.Setup: The setup project

### Building the App Itself

Building the service app and the settings app is quite straightforward.

On Windows you can simply use the _Build-Portable.ps1_ script located in the _build/windows_ directory. Bear in mind that you have to call this script from the solution root directory: `.\build\windows\Build-Portable.ps1`. It will create the _KeyboardSwitch-Portable.zip_ file in the _bin_ directory which you can then unpack into wherever you like.

On macOS building the app is not as easy as for other platforms since it requires Apple Developer ID certificates. You should have two certificates inside your Keychain: Apple Developer ID Application, and Apple Developer ID Installer. You can run the _build-pkg.sh_ script from the solution root: `./build/macos/build-pkg.sh`, but it expects certificate names and Apple ID/Team ID as environment variables. To build the uninstaller package you can run the _build-uninstaller-pkg.sh_ script from the solution root as well: `./build/macos/build-uninstaller-pkg.sh`. It also expects the installer certificate to be present.

On Linux you should use the _build-tar.sh_ script to build the tar.gz file, the _build-deb.sh_ script to build the deb package, and the _build-rpm.sh_ script to build the RPM package. They are located in the _build/tar_, _build/deb_, and _build/rpm_ directories respectively. As with the Windows script, you have to execute these scripts from the solution root: `./build/tar/build-tar.sh`, `./build/deb/build-deb.sh`, and `./build/rpm/build-rpm.sh`. The scripts' results will be in the _bin_ directory.

Alternatively, you can build the projects using Visual Studio 2019 or later, or with the `dotnet` tool. Note however that you have to build the whole solution. The startup project is `KeyboardSwitch.Settings`, and if you build it, it won't actually build the `KeyboardSwitch` project, because the settings app doesn't directly depend on the service app.

All projects (except the installer) are built into a shared _bin_ folder located in the solution root. Again, this is because the settings app needs the service app to be in the same folder, but the projects don't depend on each other.

It's better to use `dotnet publish` than simply using the raw build results. You can look into how the build scripts call `dotnet publish`.

The installer project is excluded from the solution build sequence as it's not always needed.

### Building the Windows Installer

If you want to properly install the app on Windows, you can build the installer. Unlike the other projects, this one requires .NET Framework 4.8. This is because it's built with [WixSharp](https://github.com/oleg-shilo/wixsharp), which in turn is based on [the WiX toolset](https://wixtoolset.org), and as of version 3.11.2 WiX doesn't support .NET 6.

Simply run the build, and it will generate the MSI installer in the project's _bin_ folder. Before the build, it calls `dotnet publish` to use its output.

You'll have to build the installer from source if you want to get a Windows installer for Arm64, but there are some changes that you need to make. Firstly, you should get WiX 3.14 - it's still in development as of the time of this writing, so it's not recommended for production. Secondly, edit the _KeyboardSwitch.Windows.Setup.csproj_ file and set the `Arch` property to be `arm64`. Now you can build the installer for Arm64.
