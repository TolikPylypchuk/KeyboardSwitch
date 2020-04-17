using System.Reactive.Linq;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services
{
    internal class BlobCacheSettingsService : ISettingsService
    {
        private const string SwitchSettingsKey = "SwitchSettings";

        private readonly IBlobCache cache;
        private readonly ILogger<BlobCacheSettingsService> logger;

        public BlobCacheSettingsService(IBlobCache cache, ILogger<BlobCacheSettingsService> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<SwitchSettings> GetSwitchSettingsAsync()
        {
            this.logger.LogTrace("Getting the switch settings");
            return await this.cache.GetObject<SwitchSettings>(SwitchSettingsKey);
        }

        public async Task SaveSwitchSettingsAsync(SwitchSettings switchSettings)
        {
            this.logger.LogTrace("Setting the switch settings");
            await this.cache.InsertObject(SwitchSettingsKey, switchSettings);
        }
    }
}
