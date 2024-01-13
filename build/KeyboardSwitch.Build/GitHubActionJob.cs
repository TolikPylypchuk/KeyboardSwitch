using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

public class GitHubActionJob : GitHubActionsJob
{
    public string Id { get; set; } = String.Empty;
    public (string Name, string Value)[] Matrix { get; set; } = [];

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"{this.Id}:");

        using (writer.Indent())
        {
            writer.WriteLine($"name: {this.Name}");
            writer.WriteLine($"runs-on: {this.Image.GetValue()}");

            if (this.TimeoutMinutes > 0)
            {
                writer.WriteLine($"timeout-minutes: {this.TimeoutMinutes}");
            }

            if (!this.ConcurrencyGroup.IsNullOrWhiteSpace() || this.ConcurrencyCancelInProgress)
            {
                writer.WriteLine("concurrency:");
                using (writer.Indent())
                {
                    var group = this.ConcurrencyGroup;
                    if (group.IsNullOrWhiteSpace())
                    {
                        // create a default value that only cancels in-progress runs of the same workflow
                        // we don't fall back to github.ref which would disable multiple runs in main/master which is usually what is wanted
                        group = "${{ github.workflow }} @ ${{ github.event.pull_request.head.label || github.head_ref || github.run_id }}";
                    }

                    writer.WriteLine($"group: {group}");
                    if (this.ConcurrencyCancelInProgress)
                    {
                        writer.WriteLine("cancel-in-progress: true");
                    }
                }
            }

            if (this.Matrix.Length != 0)
            {
                writer.WriteLine("continue-on-error: true");
                writer.WriteLine("strategy:");
                using (writer.Indent())
                {
                    writer.WriteLine("fail-fast: false");
                    writer.WriteLine("matrix:");
                    using (writer.Indent())
                    {
                        foreach (var matrix in this.Matrix)
                        {
                            writer.WriteLine($"{matrix.Name}: {matrix.Value}");
                        }
                    }
                }
            }

            writer.WriteLine("steps:");
            using (writer.Indent())
            {
                Steps.ForEach(x => x.Write(writer));
            }
        }
    }
}
