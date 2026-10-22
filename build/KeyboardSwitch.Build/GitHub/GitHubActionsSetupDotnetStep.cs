using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace KeyboardSwitch.Build.GitHub;

public class GitHubActionsSetupDotnetStep : GitHubActionsStep
{
    public string SimpleName { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public string Path { get; set; } = String.Empty;
    public string? Condition { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Set up .NET");

        using (writer.Indent())
        {
            writer.WriteLine("uses: actions/setup-dotnet@v6");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                var version = Environment.Version;
                writer.WriteLine($"dotnet-version: {version.Major}.{version.Minor}.x");
            }
        }
    }
}
