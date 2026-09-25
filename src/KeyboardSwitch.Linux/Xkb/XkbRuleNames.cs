namespace KeyboardSwitch.Linux.Xkb;

[StructLayout(LayoutKind.Sequential)]
internal struct XkbRuleNames
{
    public IntPtr Rules;
    public IntPtr Model;
    public IntPtr Layout;
    public IntPtr Variant;
    public IntPtr Options;
}
