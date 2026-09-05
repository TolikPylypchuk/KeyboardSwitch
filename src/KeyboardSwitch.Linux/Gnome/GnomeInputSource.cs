namespace KeyboardSwitch.Linux.Gnome;

internal sealed record GnomeInputSource(uint Index, string XkbId, string DisplayName, string ShortName);
