using System.Collections.Generic;
using System.Runtime.Serialization;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Settings
{
    [DataContract]
    public sealed class SwitchSettings
    {
        [DataMember]
        public List<ModifierKey> ForwardModifierKeys { get; set; } = new();

        [DataMember]
        public List<ModifierKey> BackwardModifierKeys { get; set; } = new();

        [DataMember]
        public int PressCount { get; set; }

        [DataMember]
        public int WaitMilliseconds { get; set; }
    }
}
