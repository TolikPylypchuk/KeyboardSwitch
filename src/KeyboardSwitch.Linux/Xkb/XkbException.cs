namespace KeyboardSwitch.Linux.Xkb;

public sealed class XkbException : Exception
{
    public XkbException()
    { }

    public XkbException(string message)
    : base(message)
    { }

    public XkbException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
