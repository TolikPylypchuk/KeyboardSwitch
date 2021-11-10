namespace KeyboardSwitch.Core;

[Serializable]
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

    private IncompatibleAppVersionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => this.Version = info.GetValue(nameof(this.Version), typeof(Version)) as Version;

    public Version? Version { get; }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(this.Version), this.Version);
    }
}
