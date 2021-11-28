# Technical Stuff

Keyboard Switch is written using [C#](https://github.com/dotnet/csharplang) and [.NET Core 3.1](https://github.com/dotnet/core). Following is the list of technologies used in this app and some other technical aspects.

## UI

The Keyboard Switch Settings app is written using [Avalonia](https://avaloniaui.net), a cross-platform UI framework for .NET. I chose it because unlike other UI frameworks for .NET, it's cross-platform, so when I implement the app for other platforms, I won't need to make major changes to the UI.

Previous versions of the app used .NET Framework and WPF. Back then I didn't expect to have it working on different platforms.

Previous versions of the app also included a tray icon so you could see that the app is running. I removed it in version 3.0 as I don't see much value in keeping it and cluttering your tray.

## Core Logic

The service app is just an app which runs in the background. I could have made it into a Windows service, but decided against it when I read somewhere that Windows services cannot set up keyboard hooks. I didn't actually verify this info, so I'm not sure it's correct, but the background app approach works just fine, so it's likely to stay this way.

The service app calls various native methods from the Windows' `user32.dll`. It uses [Vanara](https://github.com/dahall/Vanara) to call them.

In the previous versions the service and the settings app were not separated. It was just an app running with a hidden window which showed up when you opened it. Having a hidden window always loaded is not that great of an idea, even though it used very little RAM.

The core logic of the settings app is implemented using [ReactiveUI](https://www.reactiveui.net) as the MVVM framework. I must say that it’s a real pleasure to use this framework. Also, [Splat](https://github.com/reactiveui/splat) is used for service location.

## Preferences

Preferences are stored in the user's local app data folder (under the _KeyboardSwitch_ directory). I didn't put it into the normal app data folder because that one is shared if you use the same Windows account on multiple machines, and that kind of defeats the purpose of this app being configured for each machine individually.

This app uses [Akavache](https://github.com/reactiveui/Akavache) as the engine for storing settings.

## Logging

This app uses a plain-text log file to log the stuff it’s doing and errors it encounters along the way. [Serilog](https://serilog.net) is used as the logging library. The log file is stored in the same folder as the preferences and is rolled over after reaching 500 MB. If you really want to (and know how), you can change the logging configuration in the `appsettings.json` file which is located in the same folder as the app itself. But you're better off not touching that file at all.

## Tests

There are none. Honestly, I don't even know how to approach testing, because the core logic of the app is essentially creating system-wide side effects, so don't think it's really testable. As for the settings app, well, I could test it, but that would take a lot of time, and the app is pretty simple, so I don't plan on doing it in the nearest future.

## Docs

The docs are built and hosted on [GitBook](https://www.gitbook.com).

Previously these articles were built using [Jekyll](https://jekyllrb.com) and hosted on [GitHub Pages](https://pages.github.com), and used the [Minimal Mistakes theme](https://mmistakes.github.io/minimal-mistakes).

## Building from Source

You can build Keyboard Switch from source if you like. All projects (except the Windows installer) require .NET Core 3.1 or later.

### The Projects

The app consists of 6 projects:

* KeyboardSwitch: The Keyboard Switch service itself
* KeyboardSwitch.Settings: The Keyboard Switch Settings app
* KeyboardSwitch.Settings.Core: The core logic of the settings app
* KeyboardSwitch.Common: Common functionality and interfaces for the projects
* KeyboardSwitch.Windows: The implementation of common functionality for the Windows platform
* KeyboardSwitch.Windows.Setup: The setup project

### Building the App Itself

Building the service app and the settings app is quite straightforward. You can simply use the `Build-Portable` script located in the solution root. It will create the `KeyboardSwitch-Portable.zip` file in the `bin` folder which you can then unpack into wherever you like.

Alternatively, you can build the projects using Visual Studio 2019 or later, or with the `dotnet` tool. Note however that you have to build the whole solution. The startup project is `KeyboardSwitch.Settings`, and if you build it, it won't actually build the `KeyboardSwitch` project, because the settings app doesn't directly depend on the service app.

All projects (except the installer) are built into a shared `bin` folder located in the solution root. Again, this is because the settings app needs the service app to be in the same folder, but the projects don't depend on each other.

It's better to use `dotnet publish` than simply using the raw build results. You can look into how the `Build-Portable` script calls `dotnet publish`.

The installer project is excluded from the solution build sequence as it's not always needed.

### Building the Windows Installer

If you want to properly install the app, you can build the installer. Unlike the other projects, this one requires .NET Framework 4.8. This is because it's built with [WixSharp](https://github.com/oleg-shilo/wixsharp), which in turn is based on [the WiX toolset](https://wixtoolset.org), and as of version 3.11.2 WiX doesn't support .NET Core or .NET 5.

Simply run the build, and it will generate the MSI installer in the project's `bin` folder. Before the build, it calls `dotnet publish` to use its output.
