namespace KeyboardSwitch.MacOS.Native;

internal enum CGEventSourceStateID : uint
{
    CGEventSourceStateCombinedSessionState = 0,
    CGEventSourceStateHIDSystemState = 1,
    CGEventSourceStatePrivate = UInt32.MaxValue
}
