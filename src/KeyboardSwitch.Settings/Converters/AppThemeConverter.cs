namespace KeyboardSwitch.Settings.Converters;

public sealed class AppThemeConverter : IBindingTypeConverter
{
    public int GetAffinityForObjects(Type fromType, Type toType) =>
        fromType == typeof(AppTheme) || toType == typeof(AppTheme)
            ? 10000
            : 0;

    public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
    {
        switch (from)
        {
            case AppTheme theme:
                result = Convert.AppThemeToString(theme);
                return true;
            case string str:
                result = Convert.StringToAppTheme(str);
                return true;
            default:
                result = null;
                return false;
        }
    }
}
