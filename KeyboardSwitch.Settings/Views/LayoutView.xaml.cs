using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class LayoutView : ReactiveUserControl<LayoutViewModel>
    {
        public LayoutView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.LanguageName, v => v.LanguageTextBlock.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.KeyboardName, v => v.KeyboardTextBlock.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Characters, v => v.Characters.Items)
                    .DisposeWith(disposables);

                this.ViewModel.FormChanged
                    .Select(_ => this.ViewModel)
                    .Subscribe(vm => { });
            });

            this.InitializeComponent();
        }

        private TextBlock LanguageTextBlock { get; set; } = null!;
        private TextBlock KeyboardTextBlock { get; set; } = null!;
        private ItemsControl Characters { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.LanguageTextBlock = this.FindControl<TextBlock>(nameof(this.LanguageTextBlock));
            this.KeyboardTextBlock = this.FindControl<TextBlock>(nameof(this.KeyboardTextBlock));
            this.Characters = this.FindControl<ItemsControl>(nameof(this.Characters));
        }
    }
}
