namespace KeyboardSwitch.Linux.Xorg;

internal enum XkbKeyboardSpec
{
    XkbUseCoreKbd = 0x0100,
    XkbUseCorePtr = 0x0200,
    XkbDfltXIClass = 0x0300,
    XkbDfltXIId = 0x0400,
    XkbAllXIClasses = 0x0500,
    XkbAllXIIds = 0x0600,
    XkbXINone = 0xFF00
}
