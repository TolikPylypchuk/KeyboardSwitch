namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed partial class LayoutViewModel : ReactiveForm<LayoutModel, LayoutViewModel>
{
    [Reactive]
    private string languageName = String.Empty;

    [Reactive]
    private string keyboardName = String.Empty;

    [Reactive]
    private string id = String.Empty;

    [Reactive]
    private int index;

    [Reactive]
    private string chars = String.Empty;

    [Reactive]
    private int currentCharIndex = NoIndex;

    public LayoutViewModel(
        LayoutModel layoutModel,
        ResourceManager? resourceManager = null,
        IScheduler? scheduler = null)
        : base(resourceManager, scheduler)
    {
        this.LayoutModel = layoutModel;

        this.ValidationRule(
            vm => vm.Chars,
            chars => chars != null &&
                chars.Distinct().Count(ch => ch != MissingCharacter) == chars.Count(ch => ch != MissingCharacter),
            chars => chars != null
                ? String.Format(
                    CultureInfo.InvariantCulture,
                    this.ResourceManager.GetString("CharsDuplicatedFormat") ?? String.Empty,
                    chars
                        .Where(ch => ch != MissingCharacter)
                        .GroupBy(ch => ch)
                        .Where(chs => chs.Count() > 1)
                        .Select(chs => chs.Key.ToString())
                        .Aggregate((acc, ch) => $"{acc}, {ch}"))
                : String.Empty);

        this.CopyProperties();
        this.EnableChangeTracking();
    }

    public LayoutModel LayoutModel { get; }

    protected override LayoutViewModel Self => this;

    protected override void EnableChangeTracking()
    {
        this.TrackChanges(vm => vm.LanguageName, vm => vm.LayoutModel.LanguageName);
        this.TrackChanges(vm => vm.KeyboardName, vm => vm.LayoutModel.KeyboardName);
        this.TrackChanges(vm => vm.Id, vm => vm.LayoutModel.Id);
        this.TrackChanges(vm => vm.Index, vm => vm.LayoutModel.Index);
        this.TrackChanges(vm => vm.Chars, vm => vm.LayoutModel.Chars);

        base.EnableChangeTracking();
    }

    protected override Task<LayoutModel> OnSaveAsync()
    {
        this.LayoutModel.LanguageName = this.LanguageName;
        this.LayoutModel.KeyboardName = this.KeyboardName;
        this.LayoutModel.Id = this.Id;
        this.LayoutModel.Index = this.Index;
        this.LayoutModel.Chars = this.Chars;
        this.LayoutModel.IsNew = this.IsNew = false;

        return Task.FromResult(this.LayoutModel);
    }

    protected override void CopyProperties()
    {
        this.LanguageName = this.LayoutModel.LanguageName;
        this.KeyboardName = this.LayoutModel.KeyboardName;
        this.Id = this.LayoutModel.Id;
        this.Index = this.LayoutModel.Index;
        this.IsNew = this.LayoutModel.IsNew;
        this.Chars = this.LayoutModel.Chars;
    }
}
