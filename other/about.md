# About

Keyboard Switch. Version 4.3.1. Created by [Tolik Pylypchuk](https://github.com/TolikPylypchuk).

## Acknowledgements

The UI for the settings app was created using [Avalonia](https://avaloniaui.net) with [ReactiveUI](https://www.reactiveui.net), [FluentAvalonia](https://github.com/amwx/FluentAvalonia), and the [Devolutions macOS theme](https://github.com/Devolutions/avalonia-extensions). The global keyboard hook is implemented using [libuiohook](https://github.com/kwhat/libuiohook). Windows installer created with [WiX Toolset](https://wixtoolset.org). Docs are built using [GitBook](https://www.gitbook.com). Icon made by [Smashicons](https://smashicons.com) from [www.flaticon.com](https://www.flaticon.com).

## Changelog

### [v4.3.1](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v4.3.1) (July 20, 2025)

* When Keyboard Switch is running on Windows, it doesn't cause some editors to insert random characters when <kbd>Alt</kbd>+<kbd>Arrow</kbd> keys are pressed anymore

### [v4.3.0](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v4.3.0) (June 30, 2025)

* The settings app now supports different themes for different OSes
* Dark mode is now available for the settings app
* The preferences tab is now the default tab in the settings app
* The application now works correctly when multiple users are logged in at the same time
* The application doesn't interfere with Alt codes on Windows anymore

### [v4.2.0](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v4.2.0) (August 14, 2024)

* Arm64 is now officially supported for Windows and Linux
* The converter has been removed
* A single modifier key can now be configured as the magic key for Keyboard Switch
* The app now integrates with the clipboard directly on Linux, without xsel, but this may not work well on older desktop environments, so integration through xsel is still possible
* The internal format of the settings was changed, so when updating to version 4.2 from older versions, the settings will be reverted to the defaults – you should reconfigure the app if you had custom configuration before
* Windows installer doesn't delete all users' configurations when uninstalling the app anymore
* Purging configuration is now available on Linux for deb packages and the _tar.gz_ file
* The settings app now does auto-configuration by itself when being opened for the first time
* Switching layouts now works on GNOME 45+
* The application size and startup time has been reduced
* The application has been upgraded to .NET 8 and Avalonia 11
* WiX Toolset 5.0 is now used for the Windows installer, removing the need for .NET Framework
* NUKE is now used as the build tool

### [v4.1](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v4.1) (February 10, 2022)

* The app now works on macOS
* The app now supports Arm64 in addition to x64 (but the support is experimental for Windows and Linux)
* The app now switches layouts using native methods on Linux, hence layout settings are not needed anymore
* On Windows switching backward works correctly when there are more than two layouts
* The settings app will configure the service app to run at login when opened for the first time, instead of installers doing that – this means that the service app will be configured correctly for all users
* Windows Installer can delete all users' configurations when uninstalling the app

This is the first minor release which contains new features instead of just bug fixes, and it's a big one at that.

### [v4.0](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v4.0) (November 28, 2021)

* The app now works on Linux (via X11 only; Wayland is not supported)
* The legacy hot-key mode was removed – only the modifier keys mode is available
* The UI theme is overhauled
* The converter is hidden by default
* .NET 6 is used instead of .NET Core 3.1
* The app's performance was improved, though it's still not ideal

This is the first major version of the app which is not completely rewritten, though it was significantly changed. From now on, most changes (like macOS support) will come as minor versions, and version 4 will be the major version for the foreseeable future.

### [v3.0](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v3.0) (August 4, 2020)

* A completely rewritten app based on .NET Core and working as a truly UI-less app instead of an app with a hidden window
* The settings are now a separate app written with Avalonia instead of WPF
* Tray icon was removed
* Added the ability to switch between multiple layouts of the same language (e.g. QWERTY and Dvorak for English)
* Instant switching mode is not experimental anymore and enabled by default
* Added the ability to auto-configure character mappings
* Added checking for updates

### [v2.1](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v2.1) (April 1, 2018)

Minor bug fixes

### [v2.0](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v2.0) (January 13, 2018)

* Brand new installer for the app
* Instant switching mode
* Updated readme
* Updated configuration
* Minor UI updates
* Minor bug fixes

### [v1.1](https://github.com/TolikPylypchuk/KeyboardSwitch/releases/tag/v1.1) (somewhere in 2015)

Minor bug fixes

### v1.0 (somewhere in 2015)

All the basic functionality:

* Switching text
* Starting at system startup
* Tray icon

## License

MIT License

Copyright (c) 2015 Anatoliy Pylypchuk

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
