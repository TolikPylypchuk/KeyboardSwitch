using System.Diagnostics.CodeAnalysis;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    private static T AssertFail<T>(string message)
    {
        Assert.Fail(message);
        return default!;
    }

    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "This is the signature of a logger for external tools")]
    private static void DebugOnly(OutputType type, string text) =>
        Log.Debug(text);

    private static void ResolvePlaceholders(AbsolutePath file, string architecture) =>
        file.UpdateText(text => text
            .Replace(VersionPlaceholder, Version)
            .Replace(MajorVersionPlaceholder, MajorVersion)
            .Replace(MinorVersionPlaceholder, MinorVersion)
            .Replace(ReleasePlaceholder, ReleaseNumber)
            .Replace(ArchitecturePlaceholder, architecture)
            .Replace(OutputPlaceholder, PublishOutputDirectory));

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
            .SetRuntime(this.RuntimeIdentifier)
            .SetConfiguration(this.Configuration)
            .SetPlatform(this.Platform)
            .SetProperty(nameof(TargetOS), this.TargetOS)
            .SetNoBuild(true)
            .SetOutput(PublishOutputDirectory)
            .SetSelfContained(this.IsSelfContained)
            .SetPublishSingleFile(this.PublishSingleFile));
    }

    private void Sign(AbsolutePath file, bool hardenedRuntime = false) =>
        this.CodeSign(
            $"--sign {this.AppleApplicationCertificate} --force --timestamp --no-strict " +
            $"--entitlements {this.TargetPkgEntitlementsFile} {(hardenedRuntime ? "--options=runtime" : "")} {file}",
            logger: DebugOnly);

    private T PlatformDependent<T>(T windows, T macos, T linux) =>
        this.TargetOS switch
        {
            var os when os == TargetOS.Windows => windows,
            var os when os == TargetOS.MacOS => macos,
            var os when os == TargetOS.Linux => linux,
            _ => AssertFail<T>($"Unsupported target OS: {this.TargetOS}")
        };
}
