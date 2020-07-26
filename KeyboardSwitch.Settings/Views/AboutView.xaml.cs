using System;
using System.Globalization;
using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Properties;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class AboutView : ReactiveUserControl<AboutViewModel>
    {
        public AboutView()
        {
            this.WhenActivated(disposables =>
            {
                string version = $"{this.ViewModel.AppVersion.Major}.{this.ViewModel.AppVersion.Minor}";
                this.VersionTextBlock.Text = String.Format(
                    CultureInfo.InvariantCulture, Messages.VersionFormat, version);

                this.BindCommand(this.ViewModel, vm => vm.CheckForUpdates, v => v.CheckForUpdatesButton)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TextBlock VersionTextBlock { get; set; } = null!;
        private Button CheckForUpdatesButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.VersionTextBlock = this.FindControl<TextBlock>(nameof(this.VersionTextBlock));
            this.CheckForUpdatesButton = this.FindControl<Button>(nameof(this.CheckForUpdatesButton));
        }
    }
}
