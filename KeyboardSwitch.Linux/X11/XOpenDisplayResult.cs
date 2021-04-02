namespace KeyboardSwitch.Linux.X11
{
    internal enum XOpenDisplayResult
    {
        Success = 0,
        BadLibraryVersion = 1,
        ConnectionRefused = 2,
        NonXkbServer = 3,
        BadServerVersion = 4
    }
}
