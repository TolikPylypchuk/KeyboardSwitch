using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux.DBus;

internal sealed class DedicatedDBusConnection(string? address) : DisposableService
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    private DBusConnection? connection;
    private Task? disconnected;

    public async Task<DBusConnection> Get()
    {
        this.ThrowIfDisposed();

        await this.semaphore.WaitAsync();

        try
        {
            if (this.connection is null || this.disconnected is { IsCompleted: true })
            {
                this.connection?.Dispose();
                this.connection = null;

                var connection = await Connect(address);

                this.connection = connection;
                this.disconnected = connection.DisconnectedAsync();
            }

            return this.connection;
        } finally
        {
            this.semaphore.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.connection?.Dispose();
            this.semaphore.Dispose();
        }
    }

    private static async Task<DBusConnection> Connect(string? address)
    {
        var connection = new DBusConnection(
            address ?? throw new DBusConnectFailedException("The D-Bus address is not known"));

        try
        {
            await connection.ConnectAsync();
            return connection;
        } catch
        {
            connection.Dispose();
            throw;
        }
    }
}
