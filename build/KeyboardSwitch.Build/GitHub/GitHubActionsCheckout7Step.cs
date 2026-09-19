using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Utilities;

namespace KeyboardSwitch.Build.GitHub;

/// <summary>
/// Based on <see cref="GitHubActionsCheckoutStep" />, but uses version 7 of the actions/checkout action.
/// </summary>
public class GitHubActionsCheckout7Step : GitHubActionsStep
{
    public GitHubActionsSubmodules? Submodules { get; set; }
    public bool? Lfs { get; set; }
    public uint? FetchDepth { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- uses: actions/checkout@v7");

        if (this.Submodules.HasValue || this.Lfs.HasValue || this.FetchDepth.HasValue)
        {
            using (writer.Indent())
            {
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    if (this.Submodules.HasValue)
                    {
                        writer.WriteLine($"submodules: {this.Submodules.ToString()!.ToLowerInvariant()}");
                    }

                    if (this.Lfs.HasValue)
                    {
                        writer.WriteLine($"lfs: {this.Lfs.ToString()!.ToLowerInvariant()}");
                    }

                    if (this.FetchDepth.HasValue)
                    {
                        writer.WriteLine($"fetch-depth: {this.FetchDepth}");
                    }
                }
            }
        }
    }
}
