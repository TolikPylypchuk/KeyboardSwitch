namespace KeyboardSwitch.MacOS.Native;

internal enum CFStringEncoding : uint
{
    MacRoman = 0,
    WindowsLatin1 = 0x0500,
    ISOLatin1 = 0x0201,
    NextStepLatin = 0x0B01,
    ASCII = 0x0600,
    UTF8 = 0x08000100,
    NonLossyASCII = 0x0BFF,
    UTF16 = 0x0100,
    UTF16BE = 0x10000100,
    UTF16LE = 0x14000100,
    UTF32 = 0x0c000100,
    UTF32BE = 0x18000100,
    UTF32LE = 0x1c000100
}
