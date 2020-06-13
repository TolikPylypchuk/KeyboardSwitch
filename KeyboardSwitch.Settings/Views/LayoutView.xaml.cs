using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public class LayoutView : ReactiveUserControl<LayoutViewModel>
    {
        public LayoutView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.LanguageName, v => v.LanguageTextBlock.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.KeyboardName, v => v.KeyboardTextBlock.Text)
                    .DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.Chars, v => v.CharsTextBox.Text)
                    .DisposeWith(disposables);

                this.BindValidation(this.ViewModel, vm => vm.Chars, v => v.DuplicateCharsTextBlock.Text)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TextBlock LanguageTextBlock { get; set; } = null!;
        private TextBlock KeyboardTextBlock { get; set; } = null!;
        private TextBox CharsTextBox { get; set; } = null!;
        private TextBlock DuplicateCharsTextBlock { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.LanguageTextBlock = this.FindControl<TextBlock>(nameof(this.LanguageTextBlock));
            this.KeyboardTextBlock = this.FindControl<TextBlock>(nameof(this.KeyboardTextBlock));
            this.CharsTextBox = this.FindControl<TextBox>(nameof(this.CharsTextBox));
            this.DuplicateCharsTextBlock = this.FindControl<TextBlock>(nameof(this.DuplicateCharsTextBlock));
        }
    }
}
