namespace KeyboardSwitch.Core.Keyboard;

public record KeyboardLayout(string Id, string LanguageName, string KeyboardName, string Tag);

public sealed record LoadableKeyboardLayout(string Tag, string Name);
