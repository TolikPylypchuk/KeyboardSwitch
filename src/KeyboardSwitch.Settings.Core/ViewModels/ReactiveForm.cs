using System.Linq.Expressions;

namespace KeyboardSwitch.Settings.Core.ViewModels;

public abstract partial class ReactiveForm<TModel, TForm> : ReactiveValidationObject, IReactiveForm
    where TModel : class
    where TForm : ReactiveForm<TModel, TForm>
{
    private readonly BehaviorSubject<bool> formChangedSubject = new(false);
    private readonly BehaviorSubject<bool> validSubject = new(true);
    private readonly BehaviorSubject<bool> canSaveSubject = new(false);

    private readonly List<IObservable<bool>> changesToTrack = [];
    private readonly List<IObservable<bool>> validationsToTrack = [];

    private readonly IObservable<bool> canSave;

    protected ReactiveForm(ResourceManager? resourceManager = null)
    {
        this.ResourceManager = resourceManager ?? AppLocator.Current.GetRequiredService<ResourceManager>();

        this.Valid = Observable.CombineLatest(this.validSubject, this.IsValid()).AllTrue();

        this.canSave = Observable.CombineLatest(
            Observable.CombineLatest(this.formChangedSubject, this.canSaveSubject).AnyTrue(),
            this.Valid)
            .AllTrue();
    }

    public IObservable<bool> FormChanged => this.formChangedSubject.AsObservable();

    public IObservable<bool> Valid { get; }

    public bool IsNew { get; protected set; } = false;

    protected ResourceManager ResourceManager { get; }

    protected abstract TForm Self { get; }

    protected void TrackChanges(IObservable<bool> changes) =>
        this.changesToTrack.Add(changes
            .StartWith(false)
            .Merge(this.SaveCommand.Select(_ => false))
            .Merge(this.CancelCommand.Select(_ => false)));

    protected void TrackChanges<T>(Expression<Func<TForm, T?>> property, Func<TForm, T> itemValue)
    {
        string propertyName = property.GetMemberName();

        this.TrackChanges(
            this.Self.WhenAnyValue(property)
                .Select(value => !Equals(value, itemValue(this.Self))));
    }

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
                vms.Any(vm => vm.formChangedSubject.Value || !this.IsNew && vm.IsNew))
            .Merge(this.SaveCommand.Select(_ => false))
            .Merge(this.CancelCommand.Select(_ => false));

    protected IObservable<bool> IsCollectionChangedSimple<TItem>(
        Func<TForm, ReadOnlyObservableCollection<TItem>> property,
        Func<TForm, ICollection<TItem>> itemCollection)
        where TItem : notnull =>
        property(this.Self)
            .ToObservableChangeSet()
            .ToCollection()
            .Select(items => !Enumerable.SequenceEqual(items, itemCollection(this.Self)))
            .Merge(this.SaveCommand.Select(_ => false))
            .Merge(this.CancelCommand.Select(_ => false));

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

    protected virtual void EnableChangeTracking()
    {
        var falseWhenSave = this.SaveCommand.Select(_ => false);
        var falseWhenCancel = this.CancelCommand.Select(_ => false);

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

    [ReactiveCommand(CanExecute = nameof(canSave))]
    protected abstract Task<TModel> Save();

    protected abstract void CopyProperties();

    [ReactiveCommand(CanExecute = nameof(FormChanged))]
    private void Cancel() =>
        this.CopyProperties();
}
