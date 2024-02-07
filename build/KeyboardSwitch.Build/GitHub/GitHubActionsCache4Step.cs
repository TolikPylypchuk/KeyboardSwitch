namespace KeyboardSwitch.Build.GitHub;

using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

/// <summary>
/// Based on <see cref="GitHubActionsCacheStep" />, but uses version 4 of the actions/cache action.
/// </summary>
public class GitHubActionsCache4Step : GitHubActionsStep
{
    public string[] IncludePatterns { get; set; } = [];
    public string[] ExcludePatterns { get; set; } = [];
    public string[] KeyFiles { get; set; } = [];

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: " + $"Cache: {IncludePatterns.JoinCommaSpace()}".SingleQuote());
        using (writer.Indent())
        {
            writer.WriteLine("uses: actions/cache@v4");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("path: |");
                this.IncludePatterns.ForEach(x => writer.WriteLine($"  {x}"));
                this.ExcludePatterns.ForEach(x => writer.WriteLine($"  !{x}"));
                writer.WriteLine(
                    $"key: ${{{{ runner.os }}}}-" +
                    $"${{{{ hashFiles({KeyFiles.Select(x => x.SingleQuote()).JoinCommaSpace()}) }}}}");
            }
        }
    }
}
