using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Core.Settings
{
    [DataContract]
    public sealed class ConverterSettings
    {
        public static readonly string CacheKey = "ConverterSettings";

        [DataMember]
        public List<CustomLayoutSettings> Layouts { get; set; } = new();
    }
}
