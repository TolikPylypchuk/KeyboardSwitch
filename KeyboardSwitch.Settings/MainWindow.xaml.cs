using System.Reactive.Disposables;
using System.Windows;

using KeyboardSwitch.Settings.ViewModels;

using ReactiveUI;

using SourceChord.FluentWPF;

namespace KeyboardSwitch.Settings
{
    public partial class MainWindow : AcrylicWindow, IViewFor<MainViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(MainViewModel), typeof(MainWindow));

        public MainWindow()
        {
            this.InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(v => v.ViewModel)
                    .BindTo(this, v => v.DataContext)
                    .DisposeWith(disposables);
            });
        }

        public MainViewModel ViewModel
        {
            get => (MainViewModel)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
    }
}
