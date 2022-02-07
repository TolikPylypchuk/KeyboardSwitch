# Command Line Interface

The KeyboardSwitch service app has a CLI which you can use to control it. On Windows it's not really convenient as GUI apps cannot output anything to the console, but can be used as well.

The name of the executable is `KeyboardSwitch`. This article will assume it's installed on Linux in the `/opt/keyboard-switch` directory. On macOS it's located in the `/opt/Keyboard Switch Service.app/Contents/MacOS` directory.

## Commands

### Running the App

To start KeyboardSwitch, simply run it in the terminal:

```
/opt/keyboard-switch/KeyboardSwitch &
```

The `&` makes the service app run in the background. On Windows the `&` is not needed as GUI apps always run separately from the terminal.

KeyboardSwitch is a single-instance app. If one instance is already running, and you start another one, it will immediately silently exit.

The app will exit immediately if it cannot find the settings file. This file is created when you run the settings app for the first time. This is done so that there's a lower chance that this app will run in an unconfigured state.

### Stopping the App

To stop KeyboardSwitch, run the following:

```
/opt/keyboard-switch/KeyboardSwitch --stop
```

If an instance of KeyboardSwitch is running, this command will stop it. If no instances are running, it will do nothing.

### Reloading the Settings

To reload the settings of KeyboardSwitch, run the following:

```
/opt/keyboard-switch/KeyboardSwitch --reload-settings
```

If an instance of KeyboardSwitch is running, this command will make it invalidate its current settings, and reload them. If no instances are running, it will do nothing.

### Checking Whether the App Is Running

To check whether KeyboardSwitch is running, run the following:

```
/opt/keyboard-switch/KeyboardSwitch --check
```

If an instance of KeyboardSwitch is running, this command will output `KeyboardSwitch is running`. Otherwise it will output `KeyboardSwitch is not running`.

### Showing a Help Message

To show the help message, run either of the following:

```
/opt/keyboard-switch/KeyboardSwitch --help
/opt/keyboard-switch/KeyboardSwitch -?
```

### Command Formats

The aforementioned commands can be prefixed with `--`, `-`, or `/`. For example, to stop KeyboardSwitch, you can specify `stop`, `-stop`, `--stop`, or `/stop`.

## Exit Codes

The following exit codes are available:

* 0 - The app has exited successfully.
* 1 - An unspecified error has occurred.
* 2 - The app cannot run, because its settings have an incompatible version. The app is backward-compatible with respect to settings versions - it will read the settings created by an older version and modify them to fit the current version. But it's not forward-compatible - version 4 will not work if the settings were created or updated by a newer version.
* 3 - `stop`, `reload-settings`, or `check` was specified as the command, but no instance of KeyboardSwitch is running.
* 4 - An invalid command was specified.
* 5 - The settings file was not found.
* 6 - The app doesn't have access to the accessibility API on macOS.
