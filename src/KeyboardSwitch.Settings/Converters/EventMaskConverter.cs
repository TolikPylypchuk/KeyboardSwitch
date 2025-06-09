namespace KeyboardSwitch.Settings.Converters;

public sealed class EventMaskConverter : IBindingTypeConverter
{
    public int GetAffinityForObjects(Type fromType, Type toType) =>
        fromType == typeof(EventMask) || toType == typeof(EventMask)
            ? 10000
            : 0;

    public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
    {
        switch (from)
        {
            case EventMask modifier:
                result = Convert.ModifierToString(modifier);
                return true;
            case string str:
                result = Convert.StringToModifier(str);
                return true;
            default:
                result = null;
                return false;
        }
    }
}
