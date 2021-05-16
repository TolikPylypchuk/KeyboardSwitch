using System;

namespace KeyboardSwitch.Linux.X11
{
    [Flags]
    internal enum XkbMapComponentMask
    {
        XkbKeyTypesMask = 1 << 0,
        XkbKeySymsMask = 1 << 1,
        XkbModifierMapMask = 1 << 2,
        XkbExplicitComponentsMask = 1 << 3,
        XkbKeyActionsMask = 1 << 4,
        XkbKeyBehaviorsMask = 1 << 5,
        XkbVirtualModsMask = 1 << 6,
        XkbVirtualModMapMask = 1 << 7,

        XkbAllClientInfoMask = XkbKeyTypesMask | XkbKeySymsMask | XkbModifierMapMask,

        XkbAllServerInfoMask = XkbExplicitComponentsMask | XkbKeyActionsMask | XkbKeyBehaviorsMask |
            XkbVirtualModsMask | XkbVirtualModMapMask,

        XkbAllMapComponentsMask = XkbAllClientInfoMask | XkbAllServerInfoMask
    }
}
