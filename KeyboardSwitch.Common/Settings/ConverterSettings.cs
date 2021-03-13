using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public sealed class ConverterSettings
    {
        public static readonly string CacheKey = "ConverterSettings";

        [DataMember]
        public List<CustomLayoutSettings> Layouts { get; set; } = new();
    }
}
