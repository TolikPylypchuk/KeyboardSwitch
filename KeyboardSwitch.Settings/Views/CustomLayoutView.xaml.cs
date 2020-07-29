using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public class CustomLayoutView : ReactiveUserControl<CustomLayoutViewModel>
    {
        public CustomLayoutView()
        {
            this.WhenActivated(disposables =>
            {
                this.Bind(this.ViewModel, vm => vm.Name, v => v.NameTextBox.Text)
                    ?.DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.Chars, v => v.CharsTextBox.Text)
                    ?.DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Delete, v => v.DeleteButton)
                    .DisposeWith(disposables);

                this.NameTextBox.GetObservable(TextBox.TextProperty)
                    .Skip(1)
                    .Take(1)
                    .Subscribe(_ =>
                        this.BindValidation(this.ViewModel, vm => vm.Name, v => v.NameEmptyTextBlock.Text)
                            .DisposeWith(disposables));

                this.BindValidation(this.ViewModel, vm => vm.Chars, v => v.DuplicateCharsTextBlock.Text)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TextBox NameTextBox { get; set; } = null!;
        private TextBox CharsTextBox { get; set; } = null!;
        private Button DeleteButton { get; set; } = null!;

        private TextBlock NameEmptyTextBlock { get; set; } = null!;
        private TextBlock DuplicateCharsTextBlock { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.NameTextBox = this.FindControl<TextBox>(nameof(this.NameTextBox));
            this.CharsTextBox = this.FindControl<TextBox>(nameof(this.CharsTextBox));
            this.DeleteButton = this.FindControl<Button>(nameof(this.DeleteButton));

            this.NameEmptyTextBlock = this.FindControl<TextBlock>(nameof(this.NameEmptyTextBlock));
            this.DuplicateCharsTextBlock = this.FindControl<TextBlock>(nameof(this.DuplicateCharsTextBlock));
        }
    }
}
