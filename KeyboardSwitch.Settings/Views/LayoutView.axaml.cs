using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Core;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

using static KeyboardSwitch.Settings.Core.Constants;

namespace KeyboardSwitch.Settings.Views
{
    public partial class LayoutView : ReactiveUserControl<LayoutViewModel>
    {
        public LayoutView()
        {
            this.InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.LanguageName, v => v.LanguageTextBlock.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.KeyboardName, v => v.KeyboardTextBlock.Text)
                    .DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.Chars, v => v.CharsTextBox.Text)
                    .DisposeWith(disposables);

                var focused = this.CharsTextBox.GetObservable(IsFocusedProperty).Publish();

                this.CharsTextBox.GetObservable(TextBox.CaretIndexProperty)
                    .SkipUntil(focused.Where(isFocused => isFocused))
                    .TakeUntil(focused.Where(isFocused => !isFocused))
                    .Repeat()
                    .BindTo(this.ViewModel!, vm => vm.CurrentCharIndex)
                    .DisposeWith(disposables);

                focused
                    .Select(isFocused => isFocused ? this.CharsTextBox.CaretIndex : NoIndex)
                    .BindTo(this.ViewModel!, vm => vm.CurrentCharIndex)
                    .DisposeWith(disposables);

                focused.Connect();

                this.BindValidation(this.ViewModel, vm => vm.Chars, v => v.DuplicateCharsTextBlock.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}
