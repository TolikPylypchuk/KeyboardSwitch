namespace KeyboardSwitch.Settings.Localization;

public sealed class TranslateExtension(string key) : MarkupExtension
{
    [ConstructorArgument("key")]
    public string Key { get; set; } = key;

    public bool ToUpper { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string result = Messages.ResourceManager.GetString(this.Key) ?? this.Key;
        return this.ToUpper ? result.ToUpper() : result;
    }
}
