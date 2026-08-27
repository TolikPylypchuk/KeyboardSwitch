using System.Diagnostics.CodeAnalysis;

namespace KeyboardSwitch.Settings.Converters;

public abstract class EnumFromConverter<T>(Func<T, string> conversionFunc) :
    IBindingTypeConverter, IBindingTypeConverter<T, object?>
    where T : Enum
{
    public Type FromType => typeof(T);

    public Type ToType => typeof(object);

    public int GetAffinityForObjects() =>
        10000;

    public bool TryConvert(T? from, object? conversionHint, [MaybeNullWhen(true)] out object? result)
    {
        result = from is not null ? conversionFunc(from) : null;
        return true;
    }

    public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
    {
        if (from is T value)
        {
            result = conversionFunc(value);
            return true;
        } else
        {
            result = null;
            return false;
        }
    }
}

public abstract class EnumToConverter<T>(Func<string, T> conversionFunc) :
    IBindingTypeConverter, IBindingTypeConverter<object?, T>
    where T : Enum
{
    public Type FromType => typeof(object);

    public Type ToType => typeof(T);

    public int GetAffinityForObjects() =>
        10000;

    public bool TryConvert(object? from, object? conversionHint, [MaybeNullWhen(true)] out T? result)
    {
        if (from is string value)
        {
            result = conversionFunc(value);
            return true;
        } else
        {
            result = default;
            return false;
        }
    }

    public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
    {
        if (from is string value)
        {
            result = conversionFunc(value);
            return true;
        } else
        {
            result = default;
            return false;
        }
    }
}

public sealed class AppThemeFromConverter() : EnumFromConverter<AppTheme>(Convert.AppThemeToString);

public sealed class AppThemeToConverter() : EnumToConverter<AppTheme>(Convert.StringToAppTheme);

public sealed class AppThemeVariantFromConverter() :
    EnumFromConverter<AppThemeVariant>(Convert.AppThemeVariantToString);

public sealed class AppThemeVariantToConverter() : EnumToConverter<AppThemeVariant>(Convert.StringToAppThemeVariant);

public sealed class EventMaskFromConverter() : EnumFromConverter<EventMask>(Convert.ModifierToString);

public sealed class EventMaskToConverter() : EnumToConverter<EventMask>(Convert.StringToModifier);
