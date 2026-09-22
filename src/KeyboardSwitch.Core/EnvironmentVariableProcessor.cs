using System.Diagnostics.CodeAnalysis;

namespace KeyboardSwitch.Core;

public static class EnvironmentVariableProcessor
{
    private const string ConfigPlaceholder = "$CONFIG";

    public static string GetConfigDirectory() =>
        Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") is { Length: > 0 } configHome
            ? configHome
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

    [return: NotNullIfNotNull(nameof(text))]
    public static string? Process(string? text) =>
        text is null
            ? null
            : text.Contains(ConfigPlaceholder)
                ? Process(text.Replace(ConfigPlaceholder, GetConfigDirectory()))
                : Environment.ExpandEnvironmentVariables(text);
}
