using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Resources;
using System.Threading.Tasks;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Common;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public enum ValidationType { Valid, Required }

    public abstract class ReactiveForm<TModel, TForm> : ReactiveValidationObject<TForm>, IReactiveForm
        where TModel : class
        where TForm : ReactiveForm<TModel, TForm>
    {
        private readonly BehaviorSubject<bool> formChangedSubject = new BehaviorSubject<bool>(false);
        private readonly BehaviorSubject<bool> validSubject = new BehaviorSubject<bool>(true);
        private readonly BehaviorSubject<bool> canSaveSubject = new BehaviorSubject<bool>(false);

        private readonly List<IObservable<bool>> changesToTrack = new List<IObservable<bool>>();
        private readonly List<IObservable<bool>> validationsToTrack = new List<IObservable<bool>>();

        protected ReactiveForm(ResourceManager? resourceManager = null, IScheduler? scheduler = null)
        {
            this.ResourceManager = resourceManager ?? Locator.Current.GetService<ResourceManager>();
            this.Scheduler = scheduler ?? RxApp.MainThreadScheduler;

            this.Valid = Observable.CombineLatest(this.validSubject, this.IsValid()).AllTrue();

            var canSave = Observable.CombineLatest(
                    Observable.CombineLatest(this.formChangedSubject, this.canSaveSubject).AnyTrue(),
                    this.Valid)
                .AllTrue();

            this.Save = ReactiveCommand.CreateFromTask(this.OnSaveAsync, canSave);
            this.Cancel = ReactiveCommand.Create(this.CopyProperties, this.formChangedSubject);
        }

        public IObservable<bool> FormChanged
            => this.formChangedSubject.AsObservable();

        public bool IsFormChanged
            => this.formChangedSubject.Value;

        public IObservable<bool> Valid { get; }

        public ReactiveCommand<Unit, TModel> Save { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        protected ResourceManager ResourceManager { get; }
        protected IScheduler Scheduler { get; }

        protected abstract TForm Self { get; }

        protected void TrackChanges(IObservable<bool> changes)
            => this.changesToTrack.Add(changes.StartWith(false));

        protected void TrackChanges<T>(Expression<Func<TForm, T>> property, Func<TForm, T> itemValue)
        {
            string propertyName = property.GetMemberName();

            this.TrackChanges(
                this.Self.WhenAnyValue(property)
                    .Select(value => !Equals(value, itemValue(this.Self)))
                    .Do(changed => this.Log().Debug(
                        changed ? $"{propertyName} is changed" : $"{propertyName} is unchanged")));
        }

        protected void TrackValidation(IObservable<bool> validation)
            => this.validationsToTrack.Add(validation.StartWith(true));

        protected void TrackValidationStrict(IObservable<bool> validation)
            => this.validationsToTrack.Add(validation.StartWith(false));

        protected IObservable<bool> IsCollectionChanged<TOtherForm, TOtherModel>(
            Expression<Func<TForm, ReadOnlyObservableCollection<TOtherForm>>> property,
            Func<TForm, ICollection<TOtherModel>> itemCollection)
            where TOtherForm : ReactiveForm<TOtherModel, TOtherForm>
            where TOtherModel : class
        {
            string propertyName = property.GetMemberName();

            return property.Compile()(this.Self).ToObservableChangeSet()
                .AutoRefreshOnObservable(vm => vm.FormChanged)
                .ToCollection()
                .Select(vms =>
                    vms.Count != itemCollection(this.Self).Count || vms.Any(vm => vm.IsFormChanged))
                .Do(changed => this.Log().Debug(
                    changed ? $"{propertyName} are changed" : $"{propertyName} are unchanged"));
        }

        protected IObservable<bool> IsCollectionValid<TOtherForm>(ReadOnlyObservableCollection<TOtherForm> viewModels)
            where TOtherForm : IReactiveForm
            => viewModels.ToObservableChangeSet()
                .AutoRefreshOnObservable(vm => vm.Valid)
                .ToCollection()
                .Select(vms => vms.Select(vm => vm.Valid).CombineLatest().AllTrue())
                .Switch();

        protected ValidationHelper ValidationRule<T>(Expression<Func<TForm, T>> property, Func<T, bool> validate)
        {
            var propertyName = property.GetMemberName();
            return this.Self.ValidationRule(
                property, validate, _ => this.ResourceManager.GetString($"{propertyName}Invalid"));
        }

        protected ValidationHelper ValidationRule(IObservable<bool> validation, string errorMessage)
            => this.Self.ValidationRule(
                _ => validation,
                (vm, valid) => valid ? String.Empty : this.ResourceManager.GetString(errorMessage));

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

            falseWhenSave.Subscribe(this.canSaveSubject);

            this.validationsToTrack
                .CombineLatest()
                .AllTrue()
                .Subscribe(this.validSubject);
        }

        protected abstract Task<TModel> OnSaveAsync();

        protected abstract void CopyProperties();
    }
}
