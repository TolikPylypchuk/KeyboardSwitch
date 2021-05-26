using KeyboardSwitch.Core.Services.Simulation;
using KeyboardSwitch.Linux.X11;

namespace KeyboardSwitch.Linux.Services
{
    internal class XUserActivitySimulator : IUserActivitySimulator
    {
        public void SimulateCopy() =>
            XUtil.SimulateKeyPresses(XKeySym.LeftControl, XKeySym.C);

        public void SimulatePaste() =>
            XUtil.SimulateKeyPresses(XKeySym.LeftControl, XKeySym.V);
    }
}
