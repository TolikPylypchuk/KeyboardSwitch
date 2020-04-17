using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public sealed class SwitchSettings
    {
        [DataMember]
        public List<ModifierKeys> ModifierKeys { get; set; } = new List<ModifierKeys>();

        [DataMember]
        public char Forward { get; set; }

        [DataMember]
        public char Backward { get; set; }
    }
}
