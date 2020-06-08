using System.Runtime.Serialization;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public sealed class ModifierKeysSwitchSettings
    {
        [DataMember]
        public ModifierKeys ForwardModifierKeys { get; set; }

        [DataMember]
        public ModifierKeys BackwardModifierKeys { get; set; }

        [DataMember]
        public int PressCount { get; set; }

        [DataMember]
        public int WaitMilliseconds { get; set; }
    }
}
