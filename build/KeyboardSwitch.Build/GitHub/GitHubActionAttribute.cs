using Nuke.Common.CI.GitHubActions;

namespace KeyboardSwitch.Build.GitHub;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionAttribute(string id, string name, GitHubActionsImage image) : Attribute
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public GitHubActionsImage Image { get; } = image;

    public string[] InvokedTargets { get; set; } = [];

    public string[] Parameters { get; set; } = [];
    public string[] ImportSecrets { get; set; } = [];
    public bool EnableGitHubToken { get; set; }

    public string[] Matrix { get; set; } = [];

    public bool PublishArtifacts { get; set; } = true;
    public string PublishCondition { get; set; } = String.Empty;
    public string ArtifactSuffix { get; set; } = String.Empty;

    public int TimeoutMinutes { get; set; }

    public string JobConcurrencyGroup { get; set; } = String.Empty;
    public bool JobConcurrencyCancelInProgress { get; set; }
}
