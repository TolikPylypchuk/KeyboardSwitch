namespace KeyboardSwitch.Settings.Views;

public partial class CharMappingView : ReactiveUserControl<CharMappingViewModel>
{
    public CharMappingView()
    {
        this.InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(this.ViewModel, vm => vm.Layouts, v => v.Layouts.ItemsSource)
                .DisposeWith(disposables);

            this.BindCommands(disposables);
            this.BindTextBlocks(disposables);
        });
    }

    private void BindCommands(CompositeDisposable disposables)
    {
        this.BindCommand(this.ViewModel, vm => vm.AutoConfigureCommand, v => v.AutoConfigureButton)
            .DisposeWith(disposables);

        this.ViewModel!.AutoConfigureCommand.CanExecute
            .BindTo(this, v => v.AutoConfigureButton.IsVisible)
            .DisposeWith(disposables);

        this.BindCommand(this.ViewModel, vm => vm.RemoveLayoutsCommand, v => v.RemoveLayoutsButton)
            .DisposeWith(disposables);

        this.BindCommand(this.ViewModel, vm => vm.SaveCommand, v => v.SaveButton)
            .DisposeWith(disposables);

        this.BindCommand(this.ViewModel, vm => vm.CancelCommand, v => v.CancelButton)
            .DisposeWith(disposables);

        Observable.CombineLatest(this.ViewModel.SaveCommand.CanExecute, this.ViewModel.CancelCommand.CanExecute)
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
    }
}
