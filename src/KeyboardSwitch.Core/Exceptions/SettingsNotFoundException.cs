namespace KeyboardSwitch.Core.Exceptions;

public sealed class SettingsNotFoundException : Exception
{
    public SettingsNotFoundException()
    { }

    public SettingsNotFoundException(string? message)
        : base(message)
    { }

    public SettingsNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    { }
}
