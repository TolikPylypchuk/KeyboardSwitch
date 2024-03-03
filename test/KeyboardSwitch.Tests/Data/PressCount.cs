namespace KeyboardSwitch.Tests.Data;

public sealed record PressCount(int Value);

public sealed class ArbitraryPressCount : Arbitrary<PressCount>
{
    public override Gen<PressCount> Generator =>
        from value in Gen.Choose(2, 4)
        select new PressCount(value);
}
