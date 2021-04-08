using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using static KeyboardSwitch.Core.Constants;

namespace KeyboardSwitch.Core.Services
{
    public class SwitchService : ISwitchService
    {
        private readonly ITextService textService;
        private readonly ILayoutService layoutService;
        private readonly IAppSettingsService settingsService;
        private readonly ILogger<SwitchService> logger;

        public SwitchService(
            ITextService textService,
            ILayoutService layoutService,
            IAppSettingsService settingsService,
            ILogger<SwitchService> logger)
        {
            this.textService = textService;
            this.layoutService = layoutService;
            this.settingsService = settingsService;
            this.logger = logger;
        }

        public async Task SwitchTextAsync(SwitchDirection direction)
        {
            this.logger.LogDebug($"Switching the text {direction.AsString()}");

            string? textToSwitch = await this.textService.GetTextAsync();

            var settings = await this.settingsService.GetAppSettingsAsync();

            if (!String.IsNullOrEmpty(textToSwitch))
            {
                var allLayouts = this.layoutService.GetKeyboardLayouts();

                if (direction == SwitchDirection.Backward)
                {
                    allLayouts.Reverse();
                }

                var currentLayout = this.layoutService.GetCurrentKeyboardLayout();

                var newLayout = allLayouts.SkipWhile(layout => layout != currentLayout)
                    .Skip(1)
                    .FirstOrDefault()
                    ?? allLayouts[0];

                string currentChars = settings.CharsByKeyboardLayoutId[currentLayout.Id];
                string newChars = settings.CharsByKeyboardLayoutId[newLayout.Id];

                var mapping = currentChars.Zip(newChars).ToDictionary(chars => chars.First, chars => chars.Second);

                await this.textService.SetTextAsync(new String(textToSwitch
                    .Select(ch => mapping.TryGetValue(ch, out char newCh) && newCh != MissingCharacter ? newCh : ch)
                    .ToArray()));
            }

            if (settings.SwitchLayout)
            {
                this.layoutService.SwitchCurrentLayout(direction);
            }
        }
    }
}
