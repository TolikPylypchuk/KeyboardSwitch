using System;
using System.IO;
using System.Reactive.Concurrency;

using Akavache;
using Akavache.Sqlite3;

using KeyboardSwitch.Core.Settings;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Core
{
    public static class BlobCacheFactory
    {
        public static IBlobCache CreateBlobCache(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BlobCacheFactory));

            logger.LogInformation("Spinning up the blob cache");

            var options = services.GetRequiredService<IOptions<GlobalSettings>>();
            var scheduler = services.GetService<IScheduler>();
            var path = Environment.ExpandEnvironmentVariables(options.Value.Path);

            if (!File.Exists(path))
            {
                logger.LogInformation("The blob cache file for settings not found - creating a new file");

                var file = new FileInfo(path);
                file.Directory?.Create();
                file.Create().Dispose();
            }

            return new SqlRawPersistentBlobCache(path, scheduler);
        }
    }
}
