namespace KeyboardSwitch.Core.Services.Switching;

public interface ISwitchService
{
    Task SwitchText(SwitchDirection direction);
}
