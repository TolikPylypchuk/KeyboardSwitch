using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

using KeyboardSwitch.Core.Services.Simulation;

namespace KeyboardSwitch.Windows.Services
{
    internal sealed class WinUserActivitySimulator : IUserActivitySimulator
    {
        private readonly IKeyboardSimulator keyboard;

        public WinUserActivitySimulator(IKeyboardSimulator keyboard) =>
            this.keyboard = keyboard;

        public void SimulateCopy() =>
            this.keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

        public void SimulatePaste() =>
            this.keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
    }
}
