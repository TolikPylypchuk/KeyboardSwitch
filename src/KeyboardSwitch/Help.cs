namespace KeyboardSwitch;

public static class Help
{
    private static readonly string HelpText = $@"
Keyboard Switch

An application which switches typed text
as if it were typed with another keyboard layout

If started without arguments, Keyboard Switch will create a global keyboard hook
and switch text when a configured key combination is pressed.

This is a single-instance application. Starting it without arguments
when another instance is already running will make it exit immediately.

Other functionality is available through the following options:

    stop

    Stops the execution of the main Keyboard Switch instance, if it is running.

    reload-settings

    Tells the main Keyboard Switch instance that the settings
    should be reloaded, if it is running.

    check

    Checks whether the main Keyboard Switch instance is running.

    help (or ?)

    Shows this message.

The options can be prefixed with '--', '-', or '/'.

The following exit codes are possible:

    {(int)ExitCode.Success} - the application has exited successfully.

    {(int)ExitCode.Error} - an unspecified error has occured.

    {(int)ExitCode.IncompatibleSettingsVersion} - cannot start as the settings version
    is incompatiple with the app version.

    {(int)ExitCode.KeyboardSwitchNotRunning} - 'stop', 'reload-settings', or 'check'
    was specified, but the main instance was not running.

    {(int)ExitCode.UnknownCommand} - invalid arguments were supplied.

    {(int)ExitCode.MacOSAccessibilityDisabled} - the application cannot start on macOS because
    it doesn't have access to the accessibility APIs.

Home page: https://keyboardswitch.tolik.io
Docs: https://docs.keyboardswitch.tolik.io
";

    public static ExitCode Show(TextWriter writer, ExitCode exitCode)
    {
        writer.WriteLine(HelpText);
        return exitCode;
    }
}
