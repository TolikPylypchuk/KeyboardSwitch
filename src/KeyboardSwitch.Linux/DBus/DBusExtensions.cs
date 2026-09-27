using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.DBus;

internal static class DBusExtensions
{
    extension<T>(Notification<T> notification)
    {
        public void Handle(Action<Exception?, T> handler)
        {
            if (notification.HasValue)
            {
                handler(null, notification.Value);
            } else if (notification.IsCompletion)
            {
                handler(
                    notification.Exception ??
                        new DBusConnectionException($"The D-Bus observer has completed: {notification.Type}"),
                    default!);
            }
        }
    }
}
