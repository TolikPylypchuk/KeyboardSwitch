using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.DBus;

internal static class DBusConstants
{
    public const ObserverFlags CompletionFlags =
        ObserverFlags.EmitOnConnectionClosed | ObserverFlags.EmitOnConnectionFailed;
}
