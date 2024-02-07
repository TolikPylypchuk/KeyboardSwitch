namespace KeyboardSwitch.Build.GitHub;

using System.Reflection;
using System.Runtime.CompilerServices;

using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

/// <summary>
/// Based on <see cref="GitHubActionsAttribute" />, but allows multiple job definitions in the same file.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class MultipleGitHubActionsAttribute(string fileName, string name, Type targetType)
    : ConfigurationAttributeBase
{
    private readonly string name = name;
    private readonly string fileName = fileName.Replace(oldChar: ' ', newChar: '_');

    private readonly GitHubActionAttribute[] actions =
        targetType.GetCustomAttributes<GitHubActionAttribute>().ToArray();

    private GitHubActionsSubmodules? submodules;
    private bool? lfs;
    private uint? fetchDepth;

    public override string IdPostfix =>
        this.fileName;

    public override Type HostType =>
        typeof(GitHubActions);

    public override AbsolutePath ConfigurationFile =>
        this.Build.RootDirectory / ".github" / "workflows" / $"{this.fileName}.yml";

    public override IEnumerable<AbsolutePath> GeneratedFiles =>
        new[] { this.ConfigurationFile };

    public override IEnumerable<string> RelevantTargetNames =>
        this.actions.SelectMany(a => a.InvokedTargets).Distinct();

    public override IEnumerable<string> IrrelevantTargetNames => [];

    public GitHubActionsTrigger[] On { get; set; } = [];
    public string[] OnPushBranches { get; set; } = [];
    public string[] OnPushBranchesIgnore { get; set; } = [];
    public string[] OnPushTags { get; set; } = [];
    public string[] OnPushTagsIgnore { get; set; } = [];
    public string[] OnPushIncludePaths { get; set; } = [];
    public string[] OnPushExcludePaths { get; set; } = [];
    public string[] OnPullRequestBranches { get; set; } = [];
    public string[] OnPullRequestTags { get; set; } = [];
    public string[] OnPullRequestIncludePaths { get; set; } = [];
    public string[] OnPullRequestExcludePaths { get; set; } = [];
    public bool OnWorkflowDispatch { get; set; }
    public string[] OnWorkflowDispatchOptionalInputs { get; set; } = [];
    public string[] OnWorkflowDispatchRequiredInputs { get; set; } = [];
    public string OnCronSchedule { get; set; } = String.Empty;

    public GitHubActionsPermissions[] WritePermissions { get; set; } = [];
    public GitHubActionsPermissions[] ReadPermissions { get; set; } = [];

    public string[] CacheIncludePatterns { get; set; } = [".nuke/temp", "~/.nuget/packages"];
    public string[] CacheExcludePatterns { get; set; } = [];
    public string[] CacheKeyFiles { get; set; } = ["**/global.json", "**/*.csproj", "**/Directory.Packages.props"];

    public GitHubActionsSubmodules Submodules
    {
        set => this.submodules = value;
        get => throw new NotSupportedException();
    }

    public bool Lfs
    {
        set => this.lfs = value;
        get => throw new NotSupportedException();
    }

    public uint FetchDepth
    {
        set => this.fetchDepth = value;
        get => throw new NotSupportedException();
    }

    public override CustomFileWriter CreateWriter(StreamWriter streamWriter) =>
        new(streamWriter, indentationFactor: 2, commentPrefix: "#");

    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var configuration = new GitHubActionsConfiguration
        {
            Name = this.name,
            ShortTriggers = this.On,
            DetailedTriggers = this.GetTriggers().ToArray(),
            Permissions = this.WritePermissions.Select(p => (p, "write"))
                .Concat(this.ReadPermissions.Select(p => (p, "read")))
                .ToArray(),
            Jobs = this.actions.Select(action => this.GetJob(action, relevantTargets)).ToArray()
        };

        Assert.True(configuration.ShortTriggers.Length == 0 || configuration.DetailedTriggers.Length == 0,
            $"Workflows can only define either shorthand '{nameof(this.On)}' or '{nameof(this.On)}*' triggers");

        Assert.True(configuration.ShortTriggers.Length > 0 || configuration.DetailedTriggers.Length > 0,
            $"Workflows must define either shorthand '{nameof(this.On)}' or '{nameof(this.On)}*' triggers");

        return configuration;
    }

    private GitHubActionJob GetJob(
        GitHubActionAttribute action,
        IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        return new GitHubActionJob
        {
            Id = action.Id,
            Name = action.Name,
            Matrix = this.ToTuples(action.Matrix),
            Steps = this.GetSteps(action, relevantTargets).ToArray(),
            Image = action.Image,
            TimeoutMinutes = action.TimeoutMinutes,
            ConcurrencyGroup = action.JobConcurrencyGroup,
            ConcurrencyCancelInProgress = action.JobConcurrencyCancelInProgress
        };
    }

    private IEnumerable<GitHubActionsStep> GetSteps(
        GitHubActionAttribute action,
        IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        yield return new GitHubActionsCheckout4Step
        {
            Submodules = this.submodules,
            Lfs = this.lfs,
            FetchDepth = this.fetchDepth
        };

        if (this.CacheKeyFiles.Length != 0)
        {
            yield return new GitHubActionsCache4Step
            {
                IncludePatterns = this.CacheIncludePatterns,
                ExcludePatterns = this.CacheExcludePatterns,
                KeyFiles = this.CacheKeyFiles
            };
        }

        yield return new GitHubActionsRunStep
        {
            BuildCmdPath = this.BuildCmdPath,
            InvokedTargets = action.InvokedTargets,
            Imports = this.GetImports(action).ToDictionary(x => x.Key, x => x.Value)
        };

        if (action.PublishArtifacts)
        {
            var artifacts = relevantTargets
                .Where(t => action.InvokedTargets.Contains(t.Name))
                .SelectMany(t => t.ArtifactProducts)
                .Select(p => (AbsolutePath)p)
                .Distinct()
                .ToList();

            foreach (var artifact in artifacts)
            {
                var artifactName = artifact.ToString()
                    .TrimStart(artifact.Parent.ToString())
                    .TrimStart('/', '\\')
                    .TrimStart("*.");

                yield return new GitHubActionsArtifact4Step
                {
                    SimpleName = artifactName,
                    Name = action.ArtifactSuffix.IsNullOrEmpty()
                        ? artifactName
                        : $"{artifactName}-{action.ArtifactSuffix}",
                    Path = this.Build.RootDirectory.GetUnixRelativePathTo(artifact),
                    Condition = action.PublishCondition
                };
            }
        }
    }

    private IEnumerable<(string Key, string Value)> GetImports(GitHubActionAttribute action)
    {
        foreach (var input in this.OnWorkflowDispatchOptionalInputs.Concat(this.OnWorkflowDispatchRequiredInputs))
        {
            yield return (input, $"${{{{ github.event.inputs.{input} }}}}");
        }

        static string GetSecretValue(string secret) =>
            $"${{{{ secrets.{secret.SplitCamelHumpsWithKnownWords().JoinUnderscore().ToUpperInvariant()} }}}}";

        foreach (var parameter in this.ToTuples(action.Parameters))
        {
            yield return parameter;
        }

        foreach (var secret in action.ImportSecrets)
        {
            yield return (secret, GetSecretValue(secret));
        }

        if (action.EnableGitHubToken)
        {
            yield return ("GITHUB_TOKEN", GetSecretValue("GITHUB_TOKEN"));
        }
    }

    private IEnumerable<GitHubActionsDetailedTrigger> GetTriggers()
    {
        if (this.OnPushBranches.Length > 0 ||
            this.OnPushBranchesIgnore.Length > 0 ||
            this.OnPushTags.Length > 0 ||
            this.OnPushTagsIgnore.Length > 0 ||
            this.OnPushIncludePaths.Length > 0 ||
            this.OnPushExcludePaths.Length > 0)
        {
            Assert.True(
                OnPushBranches.Length == 0 && OnPushTags.Length == 0 ||
                OnPushBranchesIgnore.Length == 0 && OnPushTagsIgnore.Length == 0,
                $"Cannot use {nameof(this.OnPushBranches)}/{nameof(this.OnPushTags)} and " +
                $"{nameof(this.OnPushBranchesIgnore)}/{nameof(this.OnPushTagsIgnore)} in combination");

            yield return new GitHubActionsVcsTrigger
            {
                Kind = GitHubActionsTrigger.Push,
                Branches = this.OnPushBranches,
                BranchesIgnore = this.OnPushBranchesIgnore,
                Tags = this.OnPushTags,
                TagsIgnore = this.OnPushTagsIgnore,
                IncludePaths = this.OnPushIncludePaths,
                ExcludePaths = this.OnPushExcludePaths
            };
        }

        if (this.OnPullRequestBranches.Length > 0 ||
            this.OnPullRequestTags.Length > 0 ||
            this.OnPullRequestIncludePaths.Length > 0 ||
            this.OnPullRequestExcludePaths.Length > 0)
        {
            yield return new GitHubActionsVcsTrigger
            {
                Kind = GitHubActionsTrigger.PullRequest,
                Branches = this.OnPullRequestBranches,
                BranchesIgnore = [],
                Tags = this.OnPullRequestTags,
                TagsIgnore = [],
                IncludePaths = this.OnPullRequestIncludePaths,
                ExcludePaths = this.OnPullRequestExcludePaths
            };
        }

        if (this.OnWorkflowDispatch)
        {
            if (this.OnWorkflowDispatchOptionalInputs.Length > 0 ||
                this.OnWorkflowDispatchRequiredInputs.Length > 0)
            {
                yield return new GitHubActionsWorkflowDispatchTrigger
                {
                    OptionalInputs = this.OnWorkflowDispatchOptionalInputs,
                    RequiredInputs = this.OnWorkflowDispatchRequiredInputs
                };
            } else
            {
                yield return new GitHubActionsWorkflowDispatchSimpleTrigger();
            }
        }

        if (!String.IsNullOrEmpty(this.OnCronSchedule))
        {
            yield return new GitHubActionsScheduledTrigger { Cron = this.OnCronSchedule };
        }
    }

    private (string Name, string Value)[] ToTuples(
        string[] values,
        [CallerArgumentExpression(nameof(values))] string expr = "")
    {
        Assert.True(values.Length % 2 == 0, $"{expr} must contain an even number of items");

        return values.Buffer(2)
            .Select(v => (v[0], v[1]))
            .ToArray();
    }
}
