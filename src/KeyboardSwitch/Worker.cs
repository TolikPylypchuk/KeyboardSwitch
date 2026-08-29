using KeyboardSwitch.Core.Exceptions;

using SharpHook;
using SharpHook.Data;

namespace KeyboardSwitch;

public sealed partial class Worker(
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
            this.LogConfiguringService();

            await this.RegisterHotKeysFromSettings();

            settingsService.SettingsInvalidated.SubscribeAsync(this.RefreshHotKeys);

            this.LogStartingServiceExecution();

            await keyboardHookService.StartHook(token);
        } catch (SettingsNotFoundException e)
        {
            this.LogSettingsFileDoesNotExist(e);
            await exitService.Exit(ExitCode.SettingsDoNotExist, token);
        } catch (IncompatibleAppVersionException e)
        {
            this.LogIncompatibleAppVersion(e, e.Version);
            await exitService.Exit(ExitCode.IncompatibleSettingsVersion, token);
        } catch (HookException e) when (e.Result == UioHookResult.ErrorAxApiDisabled)
        {
            this.LogAccessibilityApiDisabled(e);
            await exitService.Exit(ExitCode.MacOSAccessibilityDisabled, token);
        } catch (HookException e) when (e.Result == UioHookResult.ErrorAxApiRevoked)
        {
            this.LogAccessibilityApiRevoked(e);
            await exitService.Exit(ExitCode.MacOSAccessibilityDisabled, token);
        } catch (Exception e)
        {
            this.LogServiceCrashed(e);
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
        this.LogRegisteringHotKeys();
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
        this.LogRefreshingKeyRegistrations();
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
            this.LogErrorWhenStarting(e);
        }
    }

    [LoggerMessage(LogLevel.Debug, "Configuring the Keyboard Switch service")]
    private partial void LogConfiguringService();

    [LoggerMessage(LogLevel.Debug, "Starting the service execution")]
    private partial void LogStartingServiceExecution();

    [LoggerMessage(LogLevel.Critical, "The settings file does not exist - open Keyboard Switch Settings to create it")]
    private partial void LogSettingsFileDoesNotExist(Exception e);

    [LoggerMessage(
        LogLevel.Critical,
        "Incompatible app version found in settings: {Version}. " +
        "Delete the settings and let the app recreate a compatible version")]
    private partial void LogIncompatibleAppVersion(Exception e, Version? version);

    [LoggerMessage(
        LogLevel.Critical,
        "The Keyboard Switch service cannot start as it doesn't have access to the macOS Accessibility API")]
    private partial void LogAccessibilityApiDisabled(Exception e);

    [LoggerMessage(
        LogLevel.Critical,
        "The Keyboard Switch service cannot run " +
        "as its access to the macOS Accessibility API has been revoked")]
    private partial void LogAccessibilityApiRevoked(Exception e);

    [LoggerMessage(LogLevel.Critical, "Keyboard Switch service has crashed")]
    private partial void LogServiceCrashed(Exception e);

    [LoggerMessage(LogLevel.Debug, "Registering hot keys which initiate switching text")]
    private partial void LogRegisteringHotKeys();

    [LoggerMessage(LogLevel.Debug, "Refreshing the hot key registrations which initiate switching text")]
    private partial void LogRefreshingKeyRegistrations();

    [LoggerMessage(LogLevel.Error, "Error when trying to switch text")]
    private partial void LogErrorWhenStarting(Exception e);
}
