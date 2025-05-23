using System.Text.Json.Serialization;

namespace KeyboardSwitch.Core.Json;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(List<string>))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(JsonStringEnumConverter<EventMask>)])]
internal partial class KeyboardSwitchJsonContext : JsonSerializerContext;
