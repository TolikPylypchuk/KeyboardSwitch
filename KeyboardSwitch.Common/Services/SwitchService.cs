using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services
{
    internal class SwitchService : ISwitchService
    {
        private readonly ITextService textService;
        private readonly ILayoutService layoutService;
        private readonly ISettingsService settingsService;
        private readonly ILogger<SwitchService> logger;

        public SwitchService(
            ITextService text,
            ILayoutService layoutService,
            ISettingsService settingsService,
            ILogger<SwitchService> logger)
        {
            this.textService = text;
            this.layoutService = layoutService;
            this.settingsService = settingsService;
            this.logger = logger;
        }

        public async Task SwitchTextAsync(SwitchDirection direction)
        {
            this.logger.LogDebug($"Switching the text {direction.AsString()}");

            string? textToSwitch = await this.textService.GetTextAsync();

            var settings = await this.settingsService.GetSwitchSettingsAsync();

            if (!String.IsNullOrEmpty(textToSwitch))
            {
                var allLayouts = layoutService.GetKeyboardLayouts();

                if (direction == SwitchDirection.Backward)
                {
                    allLayouts.Reverse();
                }

                var currentLayout = layoutService.GetForegroundProcessKeyboardLayout();

                var newLayout = allLayouts.SkipWhile(layout => layout != currentLayout)
                    .Skip(1)
                    .FirstOrDefault()
                    ?? allLayouts[0];

                string currentChars = settings.CharsByKeyboardLayoutId[currentLayout.Id];
                string newChars = settings.CharsByKeyboardLayoutId[newLayout.Id];

                var mapping = currentChars.Zip(newChars).ToDictionary(chars => chars.First, chars => chars.Second);

                await this.textService.SetTextAsync(new String(textToSwitch
                    .Select(ch => mapping.TryGetValue(ch, out char newCh) ? newCh : ch)
                    .ToArray()));
            }

            if (settings.SwitchLayout)
            {
                layoutService.SwitchForegroundProcessLayout(direction);
            }
        }
    }
}
