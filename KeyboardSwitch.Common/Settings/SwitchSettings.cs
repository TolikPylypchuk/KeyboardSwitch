using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public sealed class SwitchSettings
    {
        public static readonly string CacheKey = "SwitchSettings";

        [DataMember]
        public ModifierKeys ModifierKeys { get; set; }

        [DataMember]
        public char Forward { get; set; }

        [DataMember]
        public char Backward { get; set; }

        [DataMember]
        public Dictionary<int, string> CharsByKeyboardLayoutId { get; set; } = new Dictionary<int, string>();

        [DataMember]
        public bool InstantSwitching { get; set; }

        [DataMember]
        public bool SwitchLayout { get; set; }

        [DataMember]
        public string ServicePath { get; set; } = String.Empty;
    }
}
