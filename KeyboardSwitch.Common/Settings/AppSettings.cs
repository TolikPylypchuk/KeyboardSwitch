using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public sealed class AppSettings
    {
        public static readonly string CacheKey = "AppSettings";

        [DataMember]
        public SwitchSettings SwitchSettings { get; set; } = null!;

        [DataMember]
        public Dictionary<string, string> CharsByKeyboardLayoutId { get; set; } = new();

        [DataMember]
        public bool InstantSwitching { get; set; }

        [DataMember]
        public bool SwitchLayout { get; set; }

        [DataMember]
        public bool ShowUninstalledLayoutsMessage { get; set; }

        [DataMember]
        public bool ShowConverter { get; set; }

        [DataMember]
        public string ServicePath { get; set; } = String.Empty;

        [DataMember]
        public Version AppVersion { get; set; } = null!;
    }
}
