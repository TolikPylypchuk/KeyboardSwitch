using System.Linq.Expressions;

namespace KeyboardSwitch.Settings.Core.ViewModels;

public abstract class ReactiveForm<TModel, TForm> : ReactiveValidationObject, IReactiveForm
    where TModel : class
    where TForm : ReactiveForm<TModel, TForm>
{
    private readonly BehaviorSubject<bool> formChangedSubject = new(false);
    private readonly BehaviorSubject<bool> validSubject = new(true);
    private readonly BehaviorSubject<bool> canSaveSubject = new(false);
    private readonly BehaviorSubject<bool> canDeleteSubject = new(false);

    private readonly List<IObservable<bool>> changesToTrack = [];
    private readonly List<IObservable<bool>> validationsToTrack = [];

    protected ReactiveForm(ResourceManager? resourceManager = null, IScheduler? scheduler = null)
    {
        this.ResourceManager = resourceManager ?? GetRequiredService<ResourceManager>();
        this.Scheduler = scheduler ?? RxApp.MainThreadScheduler;

        this.Valid = Observable.CombineLatest(this.validSubject, this.IsValid()).AllTrue();

        var canSave = Observable.CombineLatest(
            Observable.CombineLatest(this.formChangedSubject, this.canSaveSubject).AnyTrue(),
            this.Valid)
            .AllTrue();

        this.Save = ReactiveCommand.CreateFromTask(this.OnSaveAsync, canSave);
        this.Cancel = ReactiveCommand.Create(this.CopyProperties, this.formChangedSubject);
        this.Delete = ReactiveCommand.CreateFromTask(this.OnDeleteAsync, this.canDeleteSubject);
    }

    public IObservable<bool> FormChanged => this.formChangedSubject.AsObservable();
    public bool IsFormChanged => this.formChangedSubject.Value;

    public IObservable<bool> Valid { get; }

    public bool IsNew { get; protected set; } = false;
    public bool IsDeleted { get; protected set; } = false;

    public ReactiveCommand<Unit, TModel> Save { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }
    public ReactiveCommand<Unit, TModel?> Delete { get; }

    protected ResourceManager ResourceManager { get; }
    protected IScheduler Scheduler { get; }

    protected abstract TForm Self { get; }

    protected void TrackChanges(IObservable<bool> changes) =>
        this.changesToTrack.Add(changes
            .StartWith(false)
            .Merge(this.Save.Select(_ => false))
            .Merge(this.Cancel.Select(_ => false)));

    protected void TrackChanges<T>(Expression<Func<TForm, T?>> property, Func<TForm, T> itemValue)
    {
        string propertyName = property.GetMemberName();

        this.TrackChanges(
            this.Self.WhenAnyValue(property)
                .Select(value => !Equals(value, itemValue(this.Self))));
    }

    protected void TrackValidation(IObservable<bool> validation) =>
        this.validationsToTrack.Add(validation.StartWith(true));

    protected void TrackValidationStrict(IObservable<bool> validation) =>
        this.validationsToTrack.Add(validation.StartWith(false));

    protected IObservable<bool> IsCollectionChanged<TOtherForm, TOtherModel>(
        Func<TForm, ReadOnlyObservableCollection<TOtherForm>> property,
        Func<TForm, ICollection<TOtherModel>> itemCollection)
        where TOtherForm : ReactiveForm<TOtherModel, TOtherForm>
        where TOtherModel : class =>
        property(this.Self)
            .ToObservableChangeSet()
            .AutoRefreshOnObservable(vm => vm.FormChanged)
            .ToCollection()
            .Select(vms =>
                vms.Count != itemCollection(this.Self).Count ||
                vms.Any(vm => vm.IsFormChanged || !this.IsNew && vm.IsNew))
            .Merge(this.Save.Select(_ => false))
            .Merge(this.Cancel.Select(_ => false));

    protected IObservable<bool> IsCollectionChangedSimple<TItem>(
        Func<TForm, ReadOnlyObservableCollection<TItem>> property,
        Func<TForm, ICollection<TItem>> itemCollection)
        where TItem : notnull =>
        property(this.Self)
            .ToObservableChangeSet()
            .ToCollection()
            .Select(items => !Enumerable.SequenceEqual(items, itemCollection(this.Self)))
            .Merge(this.Save.Select(_ => false))
            .Merge(this.Cancel.Select(_ => false));

    protected IObservable<bool> IsCollectionValid<TOtherForm>(ReadOnlyObservableCollection<TOtherForm> viewModels)
        where TOtherForm : IReactiveForm =>
        viewModels.ToObservableChangeSet()
            .AutoRefreshOnObservable(vm => vm.Valid)
            .ToCollection()
            .Select(vms => vms.Select(vm => vm.Valid).CombineLatest().AllTrue())
            .Switch();

    protected ValidationHelper LocalizedValidationRule<T>(
        Expression<Func<TForm, T?>> property,
        Func<T?, bool> validate)
    {
        string propertyName = property.GetMemberName();
        return this.Self.ValidationRule(
            property, validate, _ => this.ResourceManager.GetString($"{propertyName}Invalid") ?? String.Empty);
    }

    protected ValidationHelper LocalizedValidationRule(IObservable<bool> validation, string errorMessage) =>
        this.Self.ValidationRule(validation, this.ResourceManager.GetString(errorMessage) ?? String.Empty);

    protected void CanDeleteWhen(IObservable<bool> canDelete) =>
        canDelete.Subscribe(this.canDeleteSubject);

    protected void CanDeleteWhenNotChanged() =>
        this.CanDeleteWhen(Observable.Return(!this.IsNew).Merge(this.FormChanged.Invert()));

    protected void CanAlwaysDelete() =>
        this.CanDeleteWhen(Observable.Return(true));

    protected void CanNeverDelete() =>
        this.CanDeleteWhen(Observable.Return(false));

    protected virtual void EnableChangeTracking()
    {
        var falseWhenSave = this.Save.Select(_ => false);
        var falseWhenCancel = this.Cancel.Select(_ => false);

        this.changesToTrack
            .CombineLatest()
            .AnyTrue()
            .Merge(falseWhenSave)
            .Merge(falseWhenCancel)
            .Subscribe(this.formChangedSubject);

        Observable.Return(this.IsNew)
            .Merge(falseWhenSave)
            .Subscribe(this.canSaveSubject);

        this.validationsToTrack
            .CombineLatest()
            .AllTrue()
            .Subscribe(this.validSubject);
    }

    protected abstract Task<TModel> OnSaveAsync();

    protected virtual Task<TModel?> OnDeleteAsync() =>
        Task.FromResult<TModel?>(null);

    protected abstract void CopyProperties();
}
