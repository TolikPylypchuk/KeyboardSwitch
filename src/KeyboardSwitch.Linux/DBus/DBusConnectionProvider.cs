namespace KeyboardSwitch.Linux.DBus;

internal sealed class DBusConnectionProvider
{
    public DedicatedDBusConnection CreateConnection(string? address) =>
        new(address);
}
