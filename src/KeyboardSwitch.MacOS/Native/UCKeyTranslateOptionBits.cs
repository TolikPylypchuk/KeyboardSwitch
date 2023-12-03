namespace KeyboardSwitch.MacOS.Native;

[Flags]
internal enum UCKeyTranslateOptionBits : uint
{
    NilOptions = 0,
    TranslateNoDeadKeysMask = 1
}
