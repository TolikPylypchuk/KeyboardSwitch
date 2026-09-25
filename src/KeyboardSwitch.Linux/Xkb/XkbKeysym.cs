namespace KeyboardSwitch.Linux.Xkb;

internal static class XkbKeysym
{
    public static char ToChar(uint keysym)
    {
        uint codePoint = XkbCommon.KeysymToUtf32(keysym);

        return codePoint <= Char.MaxValue && !Char.IsSurrogate((char)codePoint)
            ? (char)codePoint
            : '\0';
    }
}
