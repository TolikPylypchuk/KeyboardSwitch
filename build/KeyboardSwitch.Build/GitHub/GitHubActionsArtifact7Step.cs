using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace KeyboardSwitch.Build.GitHub;

/// <summary>
/// Based on <see cref="GitHubActionsArtifactStep" />, but uses version 7 of the actions/upload-artifact action.
/// </summary>
public class GitHubActionsArtifact7Step : GitHubActionsStep
{
    public string SimpleName { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public string Id { get; set; } = String.Empty;
    public string Path { get; set; } = String.Empty;
    public string? Condition { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: " + $"Publish: {this.SimpleName}".SingleQuote());

        using (writer.Indent())
        {
            if (!this.Id.IsNullOrWhiteSpace())
            {
                writer.WriteLine($"id: {this.Id}");
            }

            writer.WriteLine("uses: actions/upload-artifact@v7");

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
