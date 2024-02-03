namespace KeyboardSwitch.Core.Settings;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(AppSettings))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(JsonStringEnumConverter<ModifierMask>)])]
internal partial class AppSettingsContext : JsonSerializerContext;
