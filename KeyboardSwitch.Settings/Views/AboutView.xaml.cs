using System;

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
                this.VersionTextBlock.Text = String.Format(Messages.VersionFormat, this.ViewModel.AppVersion);
            });

            this.InitializeComponent();
        }

        private TextBlock VersionTextBlock { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.VersionTextBlock = this.FindControl<TextBlock>(nameof(this.VersionTextBlock));
        }
    }
}
