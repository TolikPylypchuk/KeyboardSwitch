namespace KeyboardSwitch.Core;

using Akavache;
using Akavache.Sqlite3;

public static class BlobCacheFactory
{
    public static IBlobCache CreateBlobCache(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BlobCacheFactory));

        logger.LogDebug("Spinning up the blob cache");

        var globalSettings = services.GetRequiredService<IOptions<GlobalSettings>>();
        var path = Environment.ExpandEnvironmentVariables(globalSettings.Value.SettingsFilePath);

        if (!File.Exists(path))
        {
            logger.LogInformation("Settings file not found - creating a new file");

            var file = new FileInfo(path);
            file.Directory?.Create();
            file.Create().Dispose();
        }

        return new SqlRawPersistentBlobCache(path, services.GetService<IScheduler>());
    }
}
