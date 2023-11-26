namespace KeyboardSwitch.Settings.Controls;

public sealed class PressedKeyEventArgs(Key key) : RoutedEventArgs
{
    public Key Key { get; } = key;
}

public sealed class KeysBox : TextBox
{
    public static readonly RoutedEvent<PressedKeyEventArgs> KeyPressedEvent =
        RoutedEvent.Register<KeysBox, PressedKeyEventArgs>(nameof(KeyPressed), RoutingStrategies.Direct);

    protected override Type StyleKeyOverride => typeof(TextBox);

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
