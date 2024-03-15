using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace KeyboardSwitch.Build.GitHub;

public class GitHubActionsWorkflowDispatchSimpleTrigger : GitHubActionsDetailedTrigger
{
    public override void Write(CustomFileWriter writer) =>
        writer.WriteLine("workflow_dispatch:");
}
