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
        writer.WriteLine("  uses: actions/setup-dotnet@v4");

        using (writer.Indent())
        {
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine($"dotnet-version: 9.0.x");
            }
        }
    }
}
