namespace KeyboardSwitch.Tests.Data;

public sealed record NonModifierKey(KeyCode Value);

public sealed class ArbitraryNonModifierKey : Arbitrary<NonModifierKey>
{
    public override Gen<NonModifierKey> Generator =>
        from key in ArbMap.Default.GeneratorFor<KeyCode>()
        where !key.ToEventMask().HasValue && key != KeyCode.VcUndefined
        select new NonModifierKey(key);
}
