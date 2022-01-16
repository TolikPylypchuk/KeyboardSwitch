namespace KeyboardSwitch.MacOS.Native;

internal enum CGEventSourceStateID : uint
{
    CombinedSessionState = 0,
    HIDSystemState = 1,
    Private = UInt32.MaxValue
}
