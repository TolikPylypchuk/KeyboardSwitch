using System.Runtime.Serialization;

using KeyboardSwitch.Common.Keyboard;

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
