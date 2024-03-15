namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed class LayoutViewModel : ReactiveForm<LayoutModel, LayoutViewModel>
{
    private string languageName = String.Empty;
    private string keyboardName = String.Empty;
    private string id = String.Empty;
    private int index;
    private string chars = String.Empty;
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

    public string LanguageName
    {
        get => this.languageName;
        set => this.RaiseAndSetIfChanged(ref this.languageName, value);
    }

    public string KeyboardName
    {
        get => this.keyboardName;
        set => this.RaiseAndSetIfChanged(ref this.keyboardName, value);
    }

    public string Id
    {
        get => this.id;
        set => this.RaiseAndSetIfChanged(ref this.id, value);
    }

    public int Index
    {
        get => this.index;
        set => this.RaiseAndSetIfChanged(ref this.index, value);
    }

    public string Chars
    {
        get => this.chars;
        set => this.RaiseAndSetIfChanged(ref this.chars, value);
    }

    public int CurrentCharIndex
    {
        get => this.currentCharIndex;
        set => this.RaiseAndSetIfChanged(ref this.currentCharIndex, value);
    }

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
