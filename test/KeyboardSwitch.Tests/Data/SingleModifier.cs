namespace KeyboardSwitch.Tests.Data;

public sealed record SingleModifier(KeyCode KeyCode, ModifierMask Mask);

public sealed class ArbitrarySingleModifier : Arbitrary<SingleModifier>
{
    public override Gen<SingleModifier> Generator =>
        Gen.Elements<SingleModifier>(
            new(KeyCode.VcLeftShift, ModifierMask.LeftShift),
            new(KeyCode.VcRightShift, ModifierMask.RightShift),
            new(KeyCode.VcLeftControl, ModifierMask.LeftCtrl),
            new(KeyCode.VcRightControl, ModifierMask.RightCtrl),
            new(KeyCode.VcLeftAlt, ModifierMask.LeftAlt),
            new(KeyCode.VcRightAlt, ModifierMask.RightAlt),
            new(KeyCode.VcLeftMeta, ModifierMask.LeftMeta),
            new(KeyCode.VcRightMeta, ModifierMask.RightMeta));
}

public sealed class ArbitraryModifiers : Arbitrary<List<SingleModifier>>
{
    public override Gen<List<SingleModifier>> Generator =>
        from modifier1 in Arb.Generate<SingleModifier>()
        from modifier2 in Arb.Generate<SingleModifier>()
        from modifier3 in Arb.Generate<SingleModifier>()
        from twoItems in Arb.Generate<bool>()
        where modifier1 != modifier2 && modifier1 != modifier3 && modifier2 != modifier3
        select (twoItems ? new List<SingleModifier> { modifier1, modifier2 } : [modifier1, modifier2, modifier3]);
}
