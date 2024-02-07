namespace KeyboardSwitch.Build.GitHub;

using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

/// <summary>
/// Based on <see cref="GitHubActionsArtifactStep" />, but uses version 4 of the actions/upload-artifact action.
/// </summary>
public class GitHubActionsArtifact4Step : GitHubActionsStep
{
    public string Name { get; set; } = String.Empty;
    public string Path { get; set; } = String.Empty;
    public string? Condition { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: " + $"Publish: {Name}".SingleQuote());
        writer.WriteLine("  uses: actions/upload-artifact@v4");

        using (writer.Indent())
        {
            if (!this.Condition.IsNullOrWhiteSpace())
            {
                writer.WriteLine($"if: {this.Condition}");
            }

            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine($"name: {this.Name}");
                writer.WriteLine($"path: {this.Path}");
            }
        }
    }
}
