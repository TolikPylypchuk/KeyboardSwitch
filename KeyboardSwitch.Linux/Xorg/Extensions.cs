namespace KeyboardSwitch.Linux.Xorg;

internal static class Extensions
{
    public static char ToChar(this XKeySym keysym)
    {
        ulong keysymValue = (ulong)keysym;
        ushort keysymShortValue = (ushort)keysymValue;

        if ((keysymValue >= 0x0020 && keysymValue <= 0x007E) ||
            (keysymValue >= 0x00A0 && keysymValue <= 0x00FF))
        {
            return (char)keysymShortValue;
        }

        if (keysym == XKeySym.KeyPadSpace)
        {
            return (char)((short)XKeySym.Space & 0x7F);
        }

        if ((keysym >= XKeySym.BackSpace && keysym <= XKeySym.Clear) ||
            (keysym >= XKeySym.KeyPadMultiply && keysym <= XKeySym.KeyPad9) ||
            keysym == XKeySym.Return || keysym == XKeySym.Escape ||
            keysym == XKeySym.Delete || keysym == XKeySym.Tab ||
            keysym == XKeySym.KeyPadEnter || keysym == XKeySym.KeyPadEqual)
        {
            return (char)(keysymShortValue & 0x7F);
        }

        if (0x01000000 <= keysymValue && keysymValue <= 0x0110FFFF)
        {
            return (char)(keysymValue - 0x01000000);
        }

        return XKeySymToChar.Mappings.ContainsKey(keysymShortValue)
            ? (char)XKeySymToChar.Mappings[keysymShortValue]
            : '\0';
    }
}
