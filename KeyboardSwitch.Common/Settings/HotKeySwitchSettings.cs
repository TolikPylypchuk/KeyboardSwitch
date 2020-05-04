using System.Runtime.Serialization;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public sealed class HotKeySwitchSettings
    {
        [DataMember]
        public ModifierKeys ModifierKeys { get; set; }

        [DataMember]
        public char Forward { get; set; }

        [DataMember]
        public char Backward { get; set; }
    }
}
