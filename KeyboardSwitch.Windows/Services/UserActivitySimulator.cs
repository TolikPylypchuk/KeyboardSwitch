using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

using KeyboardSwitch.Core.Services;

namespace KeyboardSwitch.Windows.Services
{
    public sealed class UserActivitySimulator : IUserActivitySimulator
    {
        private readonly IKeyboardSimulator keyboard;

        public UserActivitySimulator(IKeyboardSimulator keyboard) =>
            this.keyboard = keyboard;

        public void SimulateCopy() =>
            this.keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

        public void SimulatePaste() =>
            this.keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
    }
}
