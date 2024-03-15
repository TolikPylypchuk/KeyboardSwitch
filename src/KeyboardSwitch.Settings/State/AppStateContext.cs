using System.Text.Json.Serialization;

namespace KeyboardSwitch.Settings.State;

[JsonSerializable(typeof(AppState))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class AppStateContext : JsonSerializerContext;
