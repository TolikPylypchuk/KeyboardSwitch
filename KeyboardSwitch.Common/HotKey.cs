using System;

namespace KeyboardSwitch.Common
{
    public class HotKey : IEquatable<HotKey>
    {
        public HotKey(ModifierKeys modifiers, int virtualKeyCode, Guid? id = null)
        {
            this.Modifiers = modifiers;
            this.VirtualKeyCode = virtualKeyCode;
            this.Id = id;
        }

        public ModifierKeys Modifiers { get; }
        public int VirtualKeyCode { get; }
        public Guid? Id { get; }

        public override bool Equals(object? obj)
            => obj is HotKey kc && this.Equals(kc);

        public bool Equals(HotKey other)
            => !(other is null) &&
               this.VirtualKeyCode == other.VirtualKeyCode &&
               this.Modifiers == other.Modifiers;

        public override int GetHashCode()
            => HashCode.Combine(this.VirtualKeyCode, this.Modifiers);

        public override string ToString()
        {
            string formatterModifiers = this.Modifiers.ToFormattedString();
            string modifiersDescription = formatterModifiers.Length != 0
                ? $"({formatterModifiers}) "
                : String.Empty;

            return $"{modifiersDescription}{this.VirtualKeyCode}";
        }

        public static bool operator ==(HotKey left, HotKey right)
            => left?.Equals(right) ?? right is null;

        public static bool operator !=(HotKey left, HotKey right)
            => !(left == right);
    }
}
