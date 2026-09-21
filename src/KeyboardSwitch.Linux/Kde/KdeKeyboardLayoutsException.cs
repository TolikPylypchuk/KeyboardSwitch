namespace KeyboardSwitch.Linux.Kde;

public sealed class KdeKeyboardLayoutsException : Exception
{
    public KdeKeyboardLayoutsException()
    { }

    public KdeKeyboardLayoutsException(string message)
        : base(message)
    { }

    public KdeKeyboardLayoutsException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
