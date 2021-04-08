using System.Collections.Generic;
using System.Runtime.Serialization;

using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Core.Settings
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
