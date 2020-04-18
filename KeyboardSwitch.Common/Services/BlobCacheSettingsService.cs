using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services
{
    internal class BlobCacheSettingsService : DisposableService, ISettingsService, IAsyncDisposable
    {
        private readonly IBlobCache cache;
        private readonly ILogger<BlobCacheSettingsService> logger;

        public BlobCacheSettingsService(IBlobCache cache, ILogger<BlobCacheSettingsService> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<SwitchSettings> GetSwitchSettingsAsync()
        {
            this.ThrowIfDisposed();

            this.logger.LogTrace("Getting the switch settings");
            return await this.cache.GetObject<SwitchSettings>(SwitchSettings.CacheKey);
        }

        public async Task SaveSwitchSettingsAsync(SwitchSettings switchSettings)
        {
            this.ThrowIfDisposed();

            this.logger.LogTrace("Setting the switch settings");
            await this.cache.InsertObject(SwitchSettings.CacheKey, switchSettings);
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.Disposed)
            {
                this.logger.LogInformation("Shutting the blob cache down");
                await BlobCache.Shutdown();
                this.Disposed = true;
            }
        }
    }
}
