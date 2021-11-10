namespace KeyboardSwitch.Linux.Services;

internal class XUserActivitySimulator : IUserActivitySimulator
{
    public void SimulateCopy() =>
        SimulateKeyPresses(XKeySym.LeftControl, XKeySym.C);

    public void SimulatePaste() =>
        SimulateKeyPresses(XKeySym.LeftControl, XKeySym.V);
}
