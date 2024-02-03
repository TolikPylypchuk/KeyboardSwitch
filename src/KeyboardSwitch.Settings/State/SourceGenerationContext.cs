namespace KeyboardSwitch.Settings.State;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(AppState))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
