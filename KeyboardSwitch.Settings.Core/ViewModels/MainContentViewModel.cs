using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class MainContentViewModel : ReactiveObject
    {
        private readonly Subject<Unit> savedSubject = new Subject<Unit>();

        public MainContentViewModel(CharMappingModel charMappingModel, PreferencesModel preferencesModel)
        {
            this.CharMappingViewModel = new CharMappingViewModel(charMappingModel);
            this.PreferencesViewModel = new PreferencesViewModel(preferencesModel);

            this.CharMappingViewModel.Save.Discard().Subscribe(this.savedSubject);
            this.PreferencesViewModel.Save.Discard().Subscribe(this.savedSubject);
        }

        public CharMappingViewModel CharMappingViewModel { get; }
        public PreferencesViewModel PreferencesViewModel { get; }

        public IObservable<Unit> Saved
            => this.savedSubject.AsObservable();
    }
}
