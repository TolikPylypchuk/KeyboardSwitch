using System;

namespace KeyboardSwitch.Common
{
    public static class ExternalCommand
    {
        public static readonly string ReloadSettings = "reload-settings";
        public static readonly string ReloadSettingsFull = "reload-settings-full";
        public static readonly string Stop = "stop";

        public static bool IsCommand(this string str, string command)
            => String.Equals(str, command, StringComparison.OrdinalIgnoreCase);

        public static bool IsUnknownCommand(this string str)
            => !(str.IsCommand(ReloadSettings) || str.IsCommand(Stop));
    }
}
