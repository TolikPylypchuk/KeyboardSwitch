using System;

using KeyboardSwitch.Common.Keyboard;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Converters
{
    public sealed class ModifierKeyConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
            => fromType == typeof(ModifierKeys) || toType == typeof(ModifierKeys)
                ? 10000
                : 0;

        public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
        {
            switch (from)
            {
                case ModifierKeys keys:
                    result = Convert.ModifierKeysToString(keys);
                    return true;
                case string str:
                    result = Convert.StringToModifierKeys(str);
                    return true;
                default:
                    result = null;
                    return false;
            }
        }
    }
}
