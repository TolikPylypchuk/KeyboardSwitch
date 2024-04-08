using KeyboardSwitch.Core.Exceptions;

using SharpHook;
using SharpHook.Native;

namespace KeyboardSwitch;

public class Worker(
    IKeyboardHookService keyboardHookService,
    ISwitchService switchService,
    IAppSettingsService settingsService,
    IExitService exitService,
    ILogger<Worker> logger)
    : BackgroundService
{
    private IDisposable? hookSubscription;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        try
        {
            logger.LogDebug("Configuring the Keyboard Switch service");

            await this.RegisterHotKeysFromSettings();

            settingsService.SettingsInvalidated.SubscribeAsync(this.RefreshHotKeys);

            logger.LogDebug("Starting the service execution");

            await keyboardHookService.StartHook(token);
        } catch (SettingsNotFoundException e)
        {
            logger.LogCritical(e, "The settings file does not exist - open Keyboard Switch Settings to create it");
            await exitService.Exit(ExitCode.SettingsDoNotExist, token);
        } catch (IncompatibleAppVersionException e)
        {
            logger.LogCritical(
                e,
                "Incompatible app version found in settings: {Version}. " +
                "Delete the settings and let the app recreate a compatible version",
                e.Version);

            await exitService.Exit(ExitCode.IncompatibleSettingsVersion, token);
        } catch (HookException e) when (e.Result == UioHookResult.ErrorAxApiDisabled)
        {
            logger.LogCritical(
                e, "The Keyboard Switch service cannot start as it doesn't have access to the macOS accessibility API");

            await exitService.Exit(ExitCode.MacOSAccessibilityDisabled, token);
        } catch (Exception e)
        {
            logger.LogCritical(e, "Keyboard Switch service has crashed");
            await exitService.Exit(ExitCode.Error, token);
        }
    }

    public override void Dispose()
    {
        this.hookSubscription?.Dispose();
        base.Dispose();
    }

    private async Task RegisterHotKeysFromSettings()
    {
        logger.LogDebug("Registering hot keys which initiate switching text");
        var settings = await settingsService.GetAppSettings(strict: true);
        this.RegisterHotKeys(settings.SwitchSettings);
    }

    private void RegisterHotKeys(SwitchSettings settings)
    {
        keyboardHookService.Register(settings.ForwardModifiers, settings.PressCount, settings.WaitMilliseconds);
        keyboardHookService.Register(settings.BackwardModifiers, settings.PressCount, settings.WaitMilliseconds);

        this.hookSubscription = keyboardHookService.HotKeyPressed
            .Select(key => key.IsSubsetKeyOf(settings.ForwardModifiers.ToArray().Merge())
                ? SwitchDirection.Forward
                : SwitchDirection.Backward)
            .SubscribeAsync(this.SwitchText);
    }

    private async Task RefreshHotKeys()
    {
        logger.LogDebug("Refreshing the hot key registrations which initiate switching text");
        keyboardHookService.UnregisterAll();
        this.hookSubscription?.Dispose();
        await this.RegisterHotKeysFromSettings();
    }

    private async Task SwitchText(SwitchDirection direction)
    {
        try
        {
            await switchService.SwitchText(direction);
        } catch (Exception e)
        {
            logger.LogError(e, "Error when trying to switch text");
        }
    }
}
