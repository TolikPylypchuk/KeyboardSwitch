public partial class Build
{
    private static AbsolutePath AnyMsiFile =>
        ArtifactsDirectory / "*.msi";

    private AbsolutePath SourceMsiFile =>
        this.Solution.KeyboardSwitch_Windows_Installer.Path.Parent / "bin" / this.Platform.MSBuild /
            this.Configuration / "en-US" / (this.Solution.KeyboardSwitch_Windows_Installer.Name + ".msi");

    private AbsolutePath MsiFile =>
        this.WithSuffix(ArtifactsDirectory / $"KeyboardSwitch-{Version}-{this.Platform.Msi}.msi");
}
