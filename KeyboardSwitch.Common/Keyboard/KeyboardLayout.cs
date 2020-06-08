using System;
using System.Globalization;

namespace KeyboardSwitch.Common.Keyboard
{
    public class KeyboardLayout : IEquatable<KeyboardLayout>
    {
        public KeyboardLayout(int id, CultureInfo culture, string keyboardName)
        {
            this.Id = id;
            this.Culture = culture;
            this.KeyboardName = keyboardName;
        }

        public int Id { get; }

        public CultureInfo Culture { get; }
        public string KeyboardName { get; }

        public override bool Equals(object? obj)
            => obj is KeyboardLayout other && this.Equals(other);

        public bool Equals(KeyboardLayout? other)
            => other != null && this.Id == other.Id;

        public override int GetHashCode()
            => HashCode.Combine(this.Id);

        public override string ToString()
            => $"{this.Culture.DisplayName} - {this.KeyboardName}";

        public static bool operator ==(KeyboardLayout? left, KeyboardLayout? right)
            => left?.Equals(right) ?? right is null;

        public static bool operator !=(KeyboardLayout? left, KeyboardLayout? right)
            => !(left == right);
    }
}
