namespace KeyboardSwitch.Core.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public sealed class IncompatibleAppVersionException : Exception
{
    public IncompatibleAppVersionException()
    { }

    public IncompatibleAppVersionException(string? message)
        : base(message)
    { }

    public IncompatibleAppVersionException(string? message, Exception? innerException)
        : base(message, innerException)
    { }

    public IncompatibleAppVersionException(Version? version)
        => this.Version = version;

    public IncompatibleAppVersionException(Version? version, string? message)
        : base(message)
        => this.Version = version;

    public IncompatibleAppVersionException(Version? version, string? message, Exception? innerException)
        : base(message, innerException)
        => this.Version = version;

    public Version? Version { get; }
}
