namespace KeyboardSwitch.Settings.Views;

public partial class LoadableLayoutsSettingsView : ReactiveUserControl<LoadableLayoutsSettingsViewModel>
{
    public LoadableLayoutsSettingsView()
    {
        this.InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(this.ViewModel, vm => vm.AddedLayouts, v => v.Layouts.Items)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.AddableLayouts, v => v.NewLayoutComboBox.Items)
                .DisposeWith(disposables);

            this.NewLayoutComboBox.GetObservable(SelectingItemsControl.SelectionChangedEvent)
                .Where(e => e.AddedItems.Count > 0)
                .Select(e => e.AddedItems[0])
                .InvokeCommand(this.ViewModel!.AddLayout)
                .DisposeWith(disposables);

            this.NewLayoutComboBox.GetObservable(SelectingItemsControl.SelectionChangedEvent)
                .Subscribe(_ => this.NewLayoutComboBox.SelectedItem = null)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.Finish, v => v.SaveButton, Observable.Return(true))
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.Finish, v => v.CancelButton, Observable.Return(false))
                .DisposeWith(disposables);
        });
    }
}
