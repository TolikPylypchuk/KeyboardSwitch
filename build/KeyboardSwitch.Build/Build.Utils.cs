using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    private IEnumerable<Project> GetProjects(bool includeInstaller = false)
    {
        yield return this.Solution.KeyboardSwitch_Core;

        yield return this.PlatformDependent(
            windows: this.Solution.KeyboardSwitch_Windows,
            macos: this.Solution.KeyboardSwitch_MacOS,
            linux: this.Solution.KeyboardSwitch_Linux);

        yield return this.Solution.KeyboardSwitch;

        yield return this.Solution.KeyboardSwitch_Settings_Core;
        yield return this.Solution.KeyboardSwitch_Settings;

        if (includeInstaller)
        {
            yield return this.Solution.KeyboardSwitch_Windows_Installer;
        }
    }

    private void PublishProject(Project project)
    {
        Log.Information("Publishing project {Name}", project.Name);

        DotNetPublish(s => s
            .SetProject(project)
            .SetRuntime(this.RuntimeIdentifer)
            .SetConfiguration(this.Configuration)
            .SetPlatform(this.Platform)
            .SetProperty(nameof(TargetOS), this.TargetOS)
            .SetNoBuild(true)
            .SetOutput(PublishOutputDirectory)
            .SetSelfContained(this.IsSelfContained)
            .SetPublishSingleFile(this.PublishSingleFile));
    }

    private T PlatformDependent<T>(T windows, T macos, T linux) =>
        this.TargetOS switch
        {
            var os when os == TargetOS.Windows => windows,
            var os when os == TargetOS.MacOS => macos,
            var os when os == TargetOS.Linux => linux,
            _ => Fail<T>($"Unsupported target OS: {this.TargetOS}")
        };

    private T Fail<T>(string message)
    {
        Assert.Fail(message);
        return default!;
    }
}
