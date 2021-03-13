using System;

namespace KeyboardSwitch.Common.Keyboard
{
    public class HotKey : IEquatable<HotKey>
    {
        public HotKey(ModifierKeys modifiers, int virtualKeyCode)
        {
            this.Modifiers = modifiers;
            this.VirtualKeyCode = virtualKeyCode;
        }

        public ModifierKeys Modifiers { get; }
        public int VirtualKeyCode { get; }

        public override bool Equals(object? obj) =>
            obj is HotKey kc && this.Equals(kc);

        public bool Equals(HotKey? other) =>
            !(other is null) &&
               this.VirtualKeyCode == other.VirtualKeyCode &&
               this.Modifiers == other.Modifiers;

        public override int GetHashCode() =>
            HashCode.Combine(this.VirtualKeyCode, this.Modifiers);

        public override string ToString()
        {
            string formattedModifiers = this.Modifiers.ToFormattedString();
            string modifiersDescription = formattedModifiers.Length != 0
                ? $"{formattedModifiers}+"
                : String.Empty;

            return $"{modifiersDescription}{this.VirtualKeyCode}";
        }

        public static bool operator ==(HotKey? left, HotKey? right) =>
            left?.Equals(right) ?? right is null;

        public static bool operator !=(HotKey? left, HotKey? right) =>
            !(left == right);
    }
}
