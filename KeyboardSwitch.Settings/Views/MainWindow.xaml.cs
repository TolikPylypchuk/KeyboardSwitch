using System.Reactive.Disposables;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class MainWindow : ReactiveWindow<MainViewModel>
    {
        public MainWindow()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.ServiceViewModel, v => v.ServiceViewContent.Content)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private ContentControl ServiceViewContent { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.ServiceViewContent = this.FindControl<ContentControl>(nameof(this.ServiceViewContent));
        }
    }
}
