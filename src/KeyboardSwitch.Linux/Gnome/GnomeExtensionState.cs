namespace KeyboardSwitch.Linux.Gnome;

internal enum GnomeExtensionState
{
    NotLoaded = 0,

    Active = 1,
    Inactive = 2,
    Error = 3,
    OutOfDate = 4,
    Downloading = 5,
    Initialized = 6,
    Deactivating = 7,
    Activating = 8,
    Uninstalled = 99
}
