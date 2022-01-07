namespace KeyboardSwitch.Core.Services.Simulation;

public interface IUserActivitySimulator
{
    Task SimulateCopyAsync();
    Task SimulatePasteAsync();
}
