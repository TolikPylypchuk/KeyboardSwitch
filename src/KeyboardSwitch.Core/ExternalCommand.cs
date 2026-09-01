namespace KeyboardSwitch.Core;

public static class ExternalCommand
{
    public static readonly string ReloadSettings = "reload-settings";
    public static readonly string Stop = "stop";

    extension(string str)
    {
        public bool IsCommand(string command) =>
            String.Equals(str, command, StringComparison.OrdinalIgnoreCase);

        public bool IsUnknownCommand() =>
            !(str.IsCommand(ReloadSettings) || str.IsCommand(Stop));
    }
}
