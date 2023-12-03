namespace KeyboardSwitch.Core.Services.Simulation;

public interface IUserActivitySimulator
{
    Task SimulateCopy();
    Task SimulatePaste();
}
