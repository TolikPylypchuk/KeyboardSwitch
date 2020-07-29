using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class LoadableLayoutsSettingsView : ReactiveUserControl<LoadableLayoutsSettingsViewModel>
    {
        public LoadableLayoutsSettingsView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.AddedLayouts, v => v.Layouts.Items)
                    ?.DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.AddableLayouts, v => v.NewLayoutComboBox.Items)
                    ?.DisposeWith(disposables);

                this.NewLayoutComboBox.GetObservable(SelectingItemsControl.SelectionChangedEvent)
                    .Where(e => e.AddedItems.Count > 0)
                    .Select(e => e.AddedItems[0])
                    .InvokeCommand(this.ViewModel.AddLayout)
                    .DisposeWith(disposables);

                this.NewLayoutComboBox.GetObservable(SelectingItemsControl.SelectionChangedEvent)
                    .Subscribe(_ => this.NewLayoutComboBox.SelectedItem = null)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Finish, v => v.SaveButton, Observable.Return(true))
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Finish, v => v.CancelButton, Observable.Return(false))
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ItemsControl Layouts { get; set; } = null!;
        private ComboBox NewLayoutComboBox { get; set; } = null!;

        private Button SaveButton { get; set; } = null!;
        private Button CancelButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.Layouts = this.FindControl<ItemsControl>(nameof(this.Layouts));
            this.NewLayoutComboBox = this.FindControl<ComboBox>(nameof(this.NewLayoutComboBox));

            this.SaveButton = this.FindControl<Button>(nameof(this.SaveButton));
            this.CancelButton = this.FindControl<Button>(nameof(this.CancelButton));
        }
    }
}
