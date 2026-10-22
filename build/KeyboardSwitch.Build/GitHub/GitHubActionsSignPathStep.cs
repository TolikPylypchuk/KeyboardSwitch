using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace KeyboardSwitch.Build.GitHub;

public class GitHubActionsSignPathStep : GitHubActionsStep
{
    public string SignPathApiToken { get; set; } = String.Empty;
    public string SignPathOrganizationId { get; set; } = String.Empty;
    public string SignPathProjectSlug { get; set; } = String.Empty;
    public string SignPathSigningPolicySlug { get; set; } = String.Empty;
    public string SignPathArtifactConfigurationSlug { get; set; } = String.Empty;
    public string GitHubArtifactId { get; set; } = String.Empty;
    public string OutputDirectory { get; set; } = String.Empty;
    public string? Condition { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: " + $"Sign the MSI with SignPath".SingleQuote());

        using (writer.Indent())
        {
            writer.WriteLine("uses: signpath/github-action-submit-signing-request@v3");

            if (!this.Condition.IsNullOrWhiteSpace())
            {
                writer.WriteLine($"if: {this.Condition}");
            }

            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine($"api-token: {this.SignPathApiToken}");
                writer.WriteLine($"organization-id: {this.SignPathOrganizationId}");
                writer.WriteLine($"project-slug: {this.SignPathProjectSlug}");
                writer.WriteLine($"signing-policy-slug: {this.SignPathSigningPolicySlug}");
                writer.WriteLine($"artifact-configuration-slug: {this.SignPathArtifactConfigurationSlug}");
                writer.WriteLine($"github-artifact-id: {this.GitHubArtifactId}");
                writer.WriteLine($"output-artifact-directory: {this.OutputDirectory}");
                writer.WriteLine("wait-for-completion: true");

                // The default connector URL doesn't work for some reason - this value is taken from the docs
                writer.WriteLine("connector-url: https://pipelineconnector.connectors.signpath.io/GitHub/GitHubCom");
            }
        }
    }
}
