using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
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
                this.OneWayBind(this.ViewModel, vm => vm.MainContentViewModel, v => v.MainContent.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.ServiceViewModel, v => v.ServiceViewContent.Content)
                    .DisposeWith(disposables);

                this.ViewModel.OpenExternally
                    .Subscribe(this.BringToForeground)
                    .DisposeWith(disposables);

                this.GetObservable(KeyUpEvent)
                    .Select(e => e.Key)
                    .Where(key => key == Key.F1)
                    .Discard()
                    .InvokeCommand(this.ViewModel.OpenAboutTab)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private ContentControl MainContent { get; set; } = null!;
        private ContentControl ServiceViewContent { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.MainContent = this.FindControl<ContentControl>(nameof(this.MainContent));
            this.ServiceViewContent = this.FindControl<ContentControl>(nameof(this.ServiceViewContent));
        }

        private void BringToForeground()
        {
            if (!this.IsVisible)
            {
                this.Show();
            }

            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }
    }
}
