using System.IO.Abstractions;

namespace KeyboardSwitch.Linux.Kde;

internal sealed partial class KxkbConfigReader(IFileSystem fileSystem, ILogger<KxkbConfigReader> logger)
{
    private const string ConfigFileName = "kxkbrc";
    private const string ConfigDirectoryVariable = "XDG_CONFIG_HOME";
    private const string DefaultConfigDirectory = ".config";

    private const string LayoutGroup = "[Layout]";
    private const string LayoutListKey = "LayoutList";
    private const string VariantListKey = "VariantList";

    private readonly IFileSystem fileSystem = fileSystem;

    public IReadOnlyList<KxkbLayout> ReadConfiguredLayouts()
    {
        string filePath = this.GetConfigFilePath();

        this.LogReadingConfig(filePath);

        if (!this.fileSystem.File.Exists(filePath))
        {
            this.LogConfigNotFound(filePath);
            return [];
        }

        try
        {
            var entries = this.ReadLayoutGroup(filePath);

            var layouts = this.SplitList(entries.GetValueOrDefault(LayoutListKey));
            var variants = this.SplitList(entries.GetValueOrDefault(VariantListKey));

            return [.. layouts.Select((layout, index) =>
                new KxkbLayout(layout, index < variants.Count ? variants[index] : String.Empty))];
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            this.LogCouldNotReadConfig(filePath, e);
            return [];
        }
    }

    private string GetConfigFilePath()
    {
        string configDirectory = Environment.GetEnvironmentVariable(ConfigDirectoryVariable) is { Length: > 0 } dir
            ? dir
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), DefaultConfigDirectory);

        return Path.Combine(configDirectory, ConfigFileName);
    }

    private List<string> SplitList(string? value) =>
        String.IsNullOrEmpty(value) ? [] : [.. value.Split(',').Select(item => item.Trim())];

    private Dictionary<string, string> ReadLayoutGroup(string filePath)
    {
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        bool inLayoutGroup = false;

        foreach (string rawLine in this.fileSystem.File.ReadLines(filePath))
        {
            string line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('['))
            {
                if (inLayoutGroup)
                {
                    break;
                }

                inLayoutGroup = line.Equals(LayoutGroup, StringComparison.Ordinal);
            } else if (inLayoutGroup && line.IndexOf('=', StringComparison.Ordinal) is int separator and > 0)
            {
                entries[line[..separator].Trim()] = line[(separator + 1)..].Trim();
            }
        }

        return entries;
    }

    [LoggerMessage(LogLevel.Debug, "Reading the configured keyboard layouts from Plasma's config: {FilePath}")]
    private partial void LogReadingConfig(string filePath);

    [LoggerMessage(LogLevel.Warning, "Plasma's keyboard config doesn't exist: {FilePath}")]
    private partial void LogConfigNotFound(string filePath);

    [LoggerMessage(LogLevel.Warning, "Could not read Plasma's keyboard config: {FilePath}")]
    private partial void LogCouldNotReadConfig(string filePath, Exception e);
}
