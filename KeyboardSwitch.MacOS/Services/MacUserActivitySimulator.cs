namespace KeyboardSwitch.MacOS.Services;

internal class MacUserActivitySimulator : IUserActivitySimulator
{
    private const int Delay = 16;

    public async Task SimulateCopyAsync()
    {
        SimulateKeyEvent(CGKeyCode.AnsiC, CGEventFlags.MaskCommand, isPressed: true);
        await Task.Delay(Delay);

        SimulateKeyEvent(CGKeyCode.AnsiC, CGEventFlags.MaskCommand, isPressed: false);
        await Task.Delay(Delay);
    }

    public async Task SimulatePasteAsync()
    {
        SimulateKeyEvent(CGKeyCode.AnsiV, CGEventFlags.MaskCommand, isPressed: true);
        await Task.Delay(Delay);

        SimulateKeyEvent(CGKeyCode.AnsiV, CGEventFlags.MaskCommand, isPressed: false);
        await Task.Delay(Delay);
    }

    private void SimulateKeyEvent(CGKeyCode keyCode, CGEventFlags flags, bool isPressed)
    {
        using var src = CoreGraphics.CGEventSourceCreate(CGEventSourceStateID.HIDSystemState);
        using var cgEvent = CoreGraphics.CGEventCreateKeyboardEvent(src, keyCode, isPressed);

        CoreGraphics.CGEventSetFlags(cgEvent, flags);
        CoreGraphics.CGEventPost(CGEventTapLocation.HIDEventTap, cgEvent);
    }
}
