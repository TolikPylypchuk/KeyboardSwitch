using System;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Linux.X11
{
    [Serializable]
    public sealed class X11Exception : Exception
    {
        public X11Exception()
        { }

        public X11Exception(string message)
        : base(message)
        { }

        public X11Exception(string message, Exception inner)
            : base(message, inner)
        { }

        private X11Exception(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
