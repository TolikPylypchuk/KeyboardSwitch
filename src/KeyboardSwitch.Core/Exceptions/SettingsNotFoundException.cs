namespace KeyboardSwitch.Core.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
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
