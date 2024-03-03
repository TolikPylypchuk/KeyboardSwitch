namespace KeyboardSwitch.Tests.Data;

public sealed record WaitTime(int Value);

public sealed class ArbitraryWaitTime : Arbitrary<WaitTime>
{
    public override Gen<WaitTime> Generator =>
        from value in Gen.Choose(6, 10)
        select new WaitTime(value * 100);
}
