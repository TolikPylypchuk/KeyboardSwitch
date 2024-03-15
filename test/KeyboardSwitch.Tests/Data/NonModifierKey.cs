using KeyboardSwitch.Core.Keyboard;

using SharpHook.Native;

namespace KeyboardSwitch.Tests.Data;

public sealed record NonModifierKey(KeyCode Value);

public sealed class ArbitraryNonModifierKey : Arbitrary<NonModifierKey>
{
    public override Gen<NonModifierKey> Generator =>
        from key in Arb.Generate<KeyCode>()
        where !key.ToModifierMask().HasValue && key != KeyCode.VcUndefined
        select new NonModifierKey(key);
}
