using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
                this.Bind(
                    this.ViewModel, vm => vm.Character,
                    v => v.CharBox.Text,
                    ch => ch.ToString(),
                    text => text.Length > 0 ? text[0] : MissingCharacter)
                    .DisposeWith(disposables);

                this.CharBox.GetObservable(KeyDownEvent, RoutingStrategies.Tunnel)
                    .Do(e => e.Handled = true)
                    .Select(this.KeyToString)
                    .Where(str => !String.IsNullOrEmpty(str))
                    .BindTo(this, v => v.CharBox.Text)
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

        private string KeyToString(KeyEventArgs e)
            => e.Key switch
            {
                Key.Q => "Q",
                Key.W => "W",
                Key.E => "E",
                Key.R => "R",
                Key.T => "T",
                Key.Y => "Y",
                Key.U => "U",
                Key.I => "I",
                Key.O => "O",
                Key.P => "P",
                Key.OemOpenBrackets => "[",
                Key.OemCloseBrackets => "]",
                Key.A => "A",
                Key.S => "S",
                Key.D => "D",
                Key.F => "F",
                Key.G => "G",
                Key.H => "H",
                Key.J => "J",
                Key.K => "K",
                Key.L => "L",
                Key.OemSemicolon => ";",
                Key.OemQuotes => "'",
                Key.Z => "Z",
                Key.X => "X",
                Key.C => "C",
                Key.V => "V",
                Key.B => "B",
                Key.N => "N",
                Key.M => "M",
                Key.OemComma => ",",
                Key.OemPeriod => ".",
                Key.OemQuestion => "/",
                Key.OemBackslash => "\\",
                Key.OemPipe => "\\",
                Key.D1 => "1",
                Key.D2 => "2",
                Key.D3 => "3",
                Key.D4 => "4",
                Key.D5 => "5",
                Key.D6 => "6",
                Key.D7 => "7",
                Key.D8 => "8",
                Key.D9 => "9",
                Key.D0 => "0",
                Key.OemMinus => "-",
                Key.OemPlus => "=",
                _ => String.Empty
            };
    }
}
