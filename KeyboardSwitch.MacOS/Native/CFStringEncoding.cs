namespace KeyboardSwitch.MacOS.Native;

internal enum CFStringEncoding : uint
{
    CFStringEncodingMacRoman = 0,
    CFStringEncodingWindowsLatin1 = 0x0500,
    CFStringEncodingISOLatin1 = 0x0201,
    CFStringEncodingNextStepLatin = 0x0B01,
    CFStringEncodingASCII = 0x0600,
    CFStringEncodingUTF8 = 0x08000100,
    CFStringEncodingNonLossyASCII = 0x0BFF,
    CFStringEncodingUTF16 = 0x0100,
    CFStringEncodingUTF16BE = 0x10000100,
    CFStringEncodingUTF16LE = 0x14000100,
    CFStringEncodingUTF32 = 0x0c000100,
    CFStringEncodingUTF32BE = 0x18000100,
    CFStringEncodingUTF32LE = 0x1c000100
}
