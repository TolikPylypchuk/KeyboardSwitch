using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Common.Settings
{
    public enum SwitchMode { HotKey, ModifierKey }

    [DataContract]
    public sealed class AppSettings
    {
        public static readonly string CacheKey = "AppSettings";

        [DataMember]
        public HotKeySwitchSettings HotKeySwitchSettings { get; set; } = null!;

        [DataMember]
        public ModifierKeysSwitchSettings ModifierKeysSwitchSettings { get; set; } = null!;

        [DataMember]
        public SwitchMode SwitchMode { get; set; }

        [DataMember]
        public Dictionary<int, string> CharsByKeyboardLayoutId { get; set; } = new();

        [DataMember]
        public bool InstantSwitching { get; set; }

        [DataMember]
        public bool SwitchLayout { get; set; }

        [DataMember]
        public bool ShowUninstalledLayoutsMessage { get; set; }

        [DataMember]
        public string ServicePath { get; set; } = String.Empty;

        [DataMember]
        public Version AppVersion { get; set; } = null!;
    }
}
