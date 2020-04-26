using System;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public class UISettings
    {
        public static readonly string CacheKey = "UISettings";

        [DataMember]
        public string ServicePath { get; set; } = String.Empty;

        [DataMember]
        public double WindowWidth { get; set; }

        [DataMember]
        public double WindowHeight { get; set; }

        [DataMember]
        public int WindowX { get; set; }

        [DataMember]
        public int WindowY { get; set; }
    }
}
