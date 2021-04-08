using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

using KeyboardSwitch.Core.Services;

namespace KeyboardSwitch.Windows.Services
{
    public sealed class UserActivitySimulator : IUserActivitySimulator
    {
        private readonly IInputSimulator input;

        public UserActivitySimulator(IInputSimulator input) =>
            this.input = input;

        public void SimulateCopy() =>
            this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

        public void SimulatePaste() =>
            this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
    }
}
