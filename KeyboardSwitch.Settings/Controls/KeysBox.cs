using System;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace KeyboardSwitch.Settings.Controls
{
    public sealed class PressedKeyEventArgs : RoutedEventArgs
    {
        public PressedKeyEventArgs(Key key) =>
            this.Key = key;

        public Key Key { get; }
    }

    public sealed class KeysBox : TextBox, IStyleable
    {
        public static readonly RoutedEvent<PressedKeyEventArgs> KeyPressedEvent =
            RoutedEvent.Register<KeysBox, PressedKeyEventArgs>(nameof(KeyPressed), RoutingStrategies.Direct);

        Type IStyleable.StyleKey => typeof(TextBox);

        protected override void OnKeyDown(KeyEventArgs e) =>
            this.OnKeyPress(e.Key);

        protected override void OnKeyUp(KeyEventArgs e) =>
            this.OnKeyPress(e.Key);

        protected override void OnTextInput(TextInputEventArgs e)
        { }

        private void OnKeyPress(Key key) =>
            this.RaiseEvent(new PressedKeyEventArgs(key) { RoutedEvent = KeyPressedEvent });

        public event EventHandler<PressedKeyEventArgs> KeyPressed
        {
            add => this.AddHandler(KeyPressedEvent, value);
            remove => this.RemoveHandler(KeyPressedEvent, value);
        }
    }
}
