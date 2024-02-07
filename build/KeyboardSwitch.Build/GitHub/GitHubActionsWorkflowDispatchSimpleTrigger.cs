namespace KeyboardSwitch.Build.GitHub;

using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

public class GitHubActionsWorkflowDispatchSimpleTrigger : GitHubActionsDetailedTrigger
{
    public override void Write(CustomFileWriter writer) =>
        writer.WriteLine("workflow_dispatch:");
}
