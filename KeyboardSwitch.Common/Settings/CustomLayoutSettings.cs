using System;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public sealed class CustomLayoutSettings
    {
        [DataMember]
        public int SequenceNumber { get; set; }

        [DataMember]
        public string Name { get; set; } = String.Empty;

        [DataMember]
        public string Chars { get; set; } = String.Empty;
    }
}
