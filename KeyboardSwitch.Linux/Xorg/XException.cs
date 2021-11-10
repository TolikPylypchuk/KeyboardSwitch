namespace KeyboardSwitch.Linux.Xorg;

using System.Runtime.Serialization;

[Serializable]
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

    private XException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}
