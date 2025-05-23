namespace KeyboardSwitch.Tests.Data;

public sealed record SingleModifier(KeyCode KeyCode, EventMask Mask);

public sealed class ArbitrarySingleModifier : Arbitrary<SingleModifier>
{
    public override Gen<SingleModifier> Generator =>
        Gen.Elements<SingleModifier>(
            new(KeyCode.VcLeftShift, EventMask.LeftShift),
            new(KeyCode.VcRightShift, EventMask.RightShift),
            new(KeyCode.VcLeftControl, EventMask.LeftCtrl),
            new(KeyCode.VcRightControl, EventMask.RightCtrl),
            new(KeyCode.VcLeftAlt, EventMask.LeftAlt),
            new(KeyCode.VcRightAlt, EventMask.RightAlt),
            new(KeyCode.VcLeftMeta, EventMask.LeftMeta),
            new(KeyCode.VcRightMeta, EventMask.RightMeta));
}

public sealed class ArbitraryModifiers : Arbitrary<List<SingleModifier>>
{
    private readonly ArbitrarySingleModifier SingleModifier = new();

    public override Gen<List<SingleModifier>> Generator =>
        from modifier1 in SingleModifier.Generator
        from modifier2 in SingleModifier.Generator
        from modifier3 in SingleModifier.Generator
        from twoItems in ArbMap.Default.GeneratorFor<bool>()
        where modifier1 != modifier2 && modifier1 != modifier3 && modifier2 != modifier3
        select (twoItems ? new List<SingleModifier> { modifier1, modifier2 } : [modifier1, modifier2, modifier3]);
}
