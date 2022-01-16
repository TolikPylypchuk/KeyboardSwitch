namespace KeyboardSwitch.Core;

using Akavache;
using Akavache.Sqlite3;

public static class BlobCacheFactory
{
    public static IBlobCache CreateBlobCache(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BlobCacheFactory));

        logger.LogDebug("Spinning up the blob cache");

        var options = services.GetRequiredService<IOptions<GlobalSettings>>();
        var scheduler = services.GetService<IScheduler>();
        var path = Environment.ExpandEnvironmentVariables(options.Value.SettingsFilePath);

        if (!File.Exists(path))
        {
            logger.LogInformation("Settings file not found - creating a new file");

            var file = new FileInfo(path);
            file.Directory?.Create();
            file.Create().Dispose();
        }

        return new SqlRawPersistentBlobCache(path, scheduler);
    }
}
