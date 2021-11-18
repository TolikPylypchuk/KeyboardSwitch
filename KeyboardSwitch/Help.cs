namespace KeyboardSwitch;

public static class Help
{
    public static void Show(TextWriter writer)
    {
        ShowMainInfo(writer);
        ShowArguments(writer);
        ShowExitCodes(writer);
        ShowLinks(writer);
    }

    private static void ShowMainInfo(TextWriter writer)
    {
        writer.WriteLine();
        writer.WriteLine("Keyboard Switch");
        writer.WriteLine();
        writer.WriteLine("An application which switches typed text");
        writer.WriteLine("as if it were typed with another keyboard layout");
        writer.WriteLine();

        writer.WriteLine("If started without arguments, Keyboard Switch will create a global keyboard hook");
        writer.WriteLine("and switch text when a configured key combination is pressed.");
        writer.WriteLine();

        writer.WriteLine("This is a single-instance application. Starting it without arguments");
        writer.WriteLine("when another instance is already running will make it exit immediately.");
        writer.WriteLine();
    }

    private static void ShowArguments(TextWriter writer)
    {
        writer.WriteLine("Other functionality is available through the following options:");
        writer.WriteLine();

        writer.WriteLine("\tstop");
        writer.WriteLine();

        writer.WriteLine("\tStops the execution of the main Keyboard Switch instance,");
        writer.WriteLine("\tif it is running.");
        writer.WriteLine();

        writer.WriteLine("\treload-settings");
        writer.WriteLine();

        writer.WriteLine("\tTells the main Keyboard Switch instance that the settings");
        writer.WriteLine("\tshould be reloaded, if it is running.");
        writer.WriteLine();

        writer.WriteLine("\tcheck");
        writer.WriteLine();

        writer.WriteLine("\tChecks whether the main Keyboard Switch instance is running.");
        writer.WriteLine();

        writer.WriteLine("\thelp (or ?)");
        writer.WriteLine();

        writer.WriteLine("\tShows this message.");
        writer.WriteLine();

        writer.WriteLine("The options can be prefixed with '--', '-', or '/'.");
        writer.WriteLine();
    }

    private static void ShowExitCodes(TextWriter writer)
    {
        writer.WriteLine("The following exit codes are possible:");
        writer.WriteLine();

        writer.WriteLine($"\t{(int)ExitCode.Success} - the application has exited successfully");
        writer.WriteLine();

        writer.WriteLine($"\t{(int)ExitCode.Error} - an unspecified error has occured");
        writer.WriteLine();

        writer.WriteLine($"\t{(int)ExitCode.IncompatibleSettingsVersion} - cannot start as the settings version");
        writer.WriteLine("\tis incompatiple with the app version");
        writer.WriteLine();

        writer.WriteLine($"\t{(int)ExitCode.KeyboardSwitchNotRunning} - 'stop', 'reload-settings', or 'check'");
        writer.WriteLine("\twas specified, but the main instance was not running");
        writer.WriteLine();

        writer.WriteLine($"\t{(int)ExitCode.UnknownCommand} - invalid arguments were supplied");
        writer.WriteLine();
    }

    private static void ShowLinks(TextWriter writer)
    {
        writer.WriteLine("Home page: https://keyboardswitch.tolik.io");
        writer.WriteLine("Docs: https://docs.keyboardswitch.tolik.io");
        writer.WriteLine();
    }
}
