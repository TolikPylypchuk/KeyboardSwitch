using System.Diagnostics.CodeAnalysis;

namespace KeyboardSwitch.Core.Exceptions;

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
