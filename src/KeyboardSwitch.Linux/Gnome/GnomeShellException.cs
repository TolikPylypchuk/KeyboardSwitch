namespace KeyboardSwitch.Linux.Gnome;

public sealed class GnomeShellException : Exception
{
    public GnomeShellException()
    { }

    public GnomeShellException(string message)
        : base(message)
    { }

    public GnomeShellException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
