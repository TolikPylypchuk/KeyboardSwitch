using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.ReactiveUI;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Core;
using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Properties;

using ReactiveUI;

using static KeyboardSwitch.Settings.Core.Constants;

namespace KeyboardSwitch.Settings.Views
{
    public partial class CharMappingView : ReactiveUserControl<CharMappingViewModel>
    {
        public CharMappingView()
        {
            this.InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.Layouts, v => v.Layouts.Items)
                    .DisposeWith(disposables);

                this.BindCommands(disposables);
                this.BindTextBlocks(disposables);
            });
        }

        private void BindCommands(CompositeDisposable disposables)
        {
            this.BindCommand(this.ViewModel, vm => vm.AutoConfigure, v => v.AutoConfigureButton)
                .DisposeWith(disposables);

            this.ViewModel!.AutoConfigure.CanExecute
                .BindTo(this, v => v.AutoConfigureButton.IsVisible)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.RemoveLayouts, v => v.RemoveLayoutsButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                .DisposeWith(disposables);

            Observable.CombineLatest(this.ViewModel.Save.CanExecute, this.ViewModel.Cancel.CanExecute)
                .AnyTrue()
                .BindTo(this, v => v.ActionPanel.IsVisible)
                .DisposeWith(disposables);
        }

        private void BindTextBlocks(CompositeDisposable disposables)
        {
            var currentIndex = this.ViewModel!.Layouts
                .ToObservableChangeSet()
                .AutoRefresh(layout => layout.CurrentCharIndex)
                .ToCollection()
                .Select(layouts => layouts
                    .Select(layout => (int?)layout.CurrentCharIndex)
                    .FirstOrDefault(index => index != NoIndex)
                    ?? NoIndex);

            currentIndex
                .Select(index => String.Format(Messages.CurrentPositionFormat, index + 1))
                .BindTo(this, v => v.CurrentPositionTextBlock.Text)
                .DisposeWith(disposables);

            currentIndex
                .Select(index => index != NoIndex)
                .BindTo(this, v => v.CurrentPositionTextBlock.IsVisible)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel!.HasNewLayouts)
                .BindTo(this, v => v.NewLayoutsTextBlock.IsVisible)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel!.CanRemoveLayouts)
                .BindTo(this, v => v.RemoveLayoutsPanel.IsVisible)
                .DisposeWith(disposables);

            Observable.CombineLatest(
                this.WhenAnyValue(v => v.ViewModel!.HasNewLayouts),
                this.WhenAnyValue(v => v.ViewModel!.ShouldRemoveLayouts))
                .AnyTrue()
                .BindTo(this, v => v.RestartServiceTextBlock.IsVisible)
                .DisposeWith(disposables);
        }
    }
}
