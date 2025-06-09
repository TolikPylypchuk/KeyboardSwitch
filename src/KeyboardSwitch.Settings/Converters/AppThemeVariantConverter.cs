namespace KeyboardSwitch.Settings.Converters;

public sealed class AppThemeVariantConverter : IBindingTypeConverter
{
    public int GetAffinityForObjects(Type fromType, Type toType) =>
        fromType == typeof(AppThemeVariant) || toType == typeof(AppThemeVariant)
            ? 10000
            : 0;

    public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
    {
        switch (from)
        {
            case AppThemeVariant variant:
                result = Convert.AppThemeVariantToString(variant);
                return true;
            case string str:
                result = Convert.StringToAppThemeVariant(str);
                return true;
            default:
                result = null;
                return false;
        }
    }
}
