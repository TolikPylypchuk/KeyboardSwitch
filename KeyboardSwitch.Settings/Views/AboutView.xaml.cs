using System;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
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
                this.VersionTextBlock.Text = String.Format(
                    CultureInfo.InvariantCulture,
                    Messages.VersionFormat,
                    this.FormatVersion(this.ViewModel.AppVersion));

                this.BindCommand(this.ViewModel, vm => vm.CheckForUpdates, v => v.CheckForUpdatesButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.OpenDocs, v => v.ViewDocsButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.GetNewVersion, v => v.GetNewVersionButton)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.LatestVersion)
                    .Select(version => String.Format(
                        CultureInfo.InvariantCulture, Messages.NewVersionAvailable, this.FormatVersion(version)))
                    .BindTo(this, v => v.NewVersionTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindElementsVisibility(disposables);
            });

            this.InitializeComponent();
        }

        private TextBlock VersionTextBlock { get; set; } = null!;
        private Button CheckForUpdatesButton { get; set; } = null!;
        private TextBlock NoNewVersionsTextBlock { get; set; } = null!;
        private Button ViewDocsButton { get; set; } = null!;

        private TextBlock NewVersionTextBlock { get; set; } = null!;
        private Button GetNewVersionButton { get; set; } = null!;
        private TextBlock GetNewVersionHintTextBlock { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.VersionTextBlock = this.FindControl<TextBlock>(nameof(this.VersionTextBlock));
            this.CheckForUpdatesButton = this.FindControl<Button>(nameof(this.CheckForUpdatesButton));
            this.NoNewVersionsTextBlock = this.FindControl<TextBlock>(nameof(this.NoNewVersionsTextBlock));
            this.ViewDocsButton = this.FindControl<Button>(nameof(this.ViewDocsButton));

            this.NewVersionTextBlock = this.FindControl<TextBlock>(nameof(this.NewVersionTextBlock));
            this.GetNewVersionButton = this.FindControl<Button>(nameof(this.GetNewVersionButton));
            this.GetNewVersionHintTextBlock = this.FindControl<TextBlock>(nameof(this.GetNewVersionHintTextBlock));
        }

        private void BindElementsVisibility(CompositeDisposable disposables)
        {
            var newVersionAvailable = this.WhenAnyValue(v => v.ViewModel.LatestVersion)
                .Select(latestVersion => latestVersion > this.ViewModel.AppVersion);

            newVersionAvailable.Invert()
                .BindTo(this, v => v.CheckForUpdatesButton.IsVisible)
                .DisposeWith(disposables);

            this.ViewModel.CheckForUpdates
                .Select(latestVersion => latestVersion <= this.ViewModel.AppVersion)
                .BindTo(this, v => v.NoNewVersionsTextBlock.IsVisible)
                .DisposeWith(disposables);

            newVersionAvailable
                .BindTo(this, v => v.NewVersionTextBlock.IsVisible)
                .DisposeWith(disposables);

            newVersionAvailable
                .BindTo(this, v => v.GetNewVersionButton.IsVisible)
                .DisposeWith(disposables);

            newVersionAvailable
                .BindTo(this, v => v.GetNewVersionHintTextBlock.IsVisible)
                .DisposeWith(disposables);
        }

        private string FormatVersion(Version version)
            => $"{version.Major}.{version.Minor}";
    }
}
