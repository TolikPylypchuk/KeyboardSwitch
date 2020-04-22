using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services
{
    internal sealed class BlobCacheSettingsService : DisposableService, ISettingsService, IAsyncDisposable
    {
        private readonly IBlobCache cache;
        private readonly ILogger<BlobCacheSettingsService> logger;

        private SwitchSettings? switchSettings;

        public BlobCacheSettingsService(IBlobCache cache, ILogger<BlobCacheSettingsService> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<SwitchSettings> GetSwitchSettingsAsync()
        {
            this.ThrowIfDisposed();

            if (this.switchSettings == null)
            {
                this.logger.LogTrace("Getting the switch settings");
                this.switchSettings = await this.cache.GetObject<SwitchSettings>(SwitchSettings.CacheKey);
            }

            return this.switchSettings;
        }

        public async Task SaveSwitchSettingsAsync(SwitchSettings switchSettings)
        {
            this.ThrowIfDisposed();

            this.logger.LogTrace("Setting the switch settings");
            await this.cache.InsertObject(SwitchSettings.CacheKey, switchSettings);

            this.switchSettings = switchSettings;
        }

        public void InvalidateSwitchSettings()
        {
            this.ThrowIfDisposed();
            this.switchSettings = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.Disposed)
            {
                this.logger.LogInformation("Shutting down the blob cache");
                await BlobCache.Shutdown();
                this.Disposed = true;
            }
        }
    }
}
