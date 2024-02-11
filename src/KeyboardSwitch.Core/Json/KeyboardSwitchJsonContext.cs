namespace KeyboardSwitch.Core.Json;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(List<string>))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(JsonStringEnumConverter<ModifierMask>)])]
internal partial class KeyboardSwitchJsonContext : JsonSerializerContext;
