using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

using KeyboardSwitch.Common.Services;

namespace KeyboardSwitch.Windows.Services
{
    public sealed class Simulator : ISimulator
    {
        private readonly IInputSimulator input;

        public Simulator(IInputSimulator input) =>
            this.input = input;

        public void SimulateCopy() =>
            this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

        public void SimulatePaste() =>
            this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
    }
}
