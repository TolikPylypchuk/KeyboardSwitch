using System;
using System.Collections.Generic;
using System.Linq;

using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Properties;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Converters
{
    public sealed class SwitchModeConverter : IBindingTypeConverter
    {
        private readonly Dictionary<SwitchMode, string> modeToString;
        private readonly Dictionary<string, SwitchMode> stringToMode;

        public SwitchModeConverter()
        {
            this.modeToString = new Dictionary<SwitchMode, string>
            {
                [SwitchMode.HotKey] = Messages.HotKey,
                [SwitchMode.ModifierKey] = Messages.ModifierKeys
            };

            this.stringToMode = modeToString.ToDictionary(e => e.Value, e => e.Key);
        }

        public int GetAffinityForObjects(Type fromType, Type toType) =>
            fromType == typeof(SwitchMode) || toType == typeof(SwitchMode)
                ? 10000
                : 0;

        public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
        {
            switch (from)
            {
                case SwitchMode mode:
                    result = this.modeToString[mode];
                    return true;
                case string str:
                    result = this.stringToMode[str];
                    return true;
                default:
                    result = null;
                    return false;
            }
        }
    }
}
