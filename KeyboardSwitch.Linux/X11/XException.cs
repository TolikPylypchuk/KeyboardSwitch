namespace KeyboardSwitch.Linux.X11;

public sealed class XException : Exception
{
    public XException()
    { }

    public XException(string message)
    : base(message)
    { }

    public XException(string message, Exception inner)
        : base(message, inner)
    { }
}
