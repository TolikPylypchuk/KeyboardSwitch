using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

using static KeyboardSwitch.Common.Constants;

namespace KeyboardSwitch.Settings.Views
{
    public class CharacterView : ReactiveUserControl<CharacterViewModel>
    {
        public CharacterView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.Bind(
                    this.ViewModel, vm => vm.Character,
                    v => v.CharBox.Text,
                    ch => ch.ToString(),
                    text => text.Length > 0 ? text[0] : MissingCharacter)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TextBox CharBox { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.CharBox = this.FindControl<TextBox>(nameof(this.CharBox));
        }
    }
}
