using System.Reactive.Disposables;

using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

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

                this.BindValidation(this.ViewModel, vm => vm.Chars, v => v.DuplicateCharsTextBlock.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}
