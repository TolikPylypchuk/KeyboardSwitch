using System;

using KeyboardSwitch.Core.Keyboard;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Converters
{
    public sealed class ModifierKeyConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType) =>
            fromType == typeof(ModifierKey) || toType == typeof(ModifierKey)
                ? 10000
                : 0;

        public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
        {
            switch (from)
            {
                case ModifierKey key:
                    result = Convert.ModifierKeyToString(key);
                    return true;
                case string str:
                    result = Convert.StringToModifierKey(str);
                    return true;
                default:
                    result = null;
                    return false;
            }
        }
    }
}
