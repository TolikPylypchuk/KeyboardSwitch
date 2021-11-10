namespace KeyboardSwitch.Settings.Core.Models;

public sealed class CustomLayoutModel : IEquatable<CustomLayoutModel>
{
    [Reactive]
    public int Id { get; set; }

    [Reactive]
    public string Name { get; set; } = String.Empty;

    [Reactive]
    public string Chars { get; set; } = String.Empty;

    public override bool Equals(object? obj) =>
        obj is CustomLayoutModel other && this.Equals(other);

    public bool Equals(CustomLayoutModel? other) =>
        !(other is null) && this.Name == other.Name && this.Chars == other.Chars;

    public override int GetHashCode() =>
        HashCode.Combine(this.Name, this.Chars);

    public override string ToString() =>
        $"{this.Name}: {this.Chars}";

    public static bool operator ==(CustomLayoutModel? left, CustomLayoutModel? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(CustomLayoutModel? left, CustomLayoutModel? right) =>
        !(left == right);
}
