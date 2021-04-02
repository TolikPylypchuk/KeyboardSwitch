using System;

namespace KeyboardSwitch.Linux.X11
{
    [Flags]
    internal enum XNamesComponentMask : uint
    {
        XkbKeycodesNameMask = 1 << 0,
        XkbGeometryNameMask = 1 << 1,
        XkbSymbolsNameMask = 1 << 2,
        XkbPhysSymbolsNameMask = 1 << 3,
        XkbTypesNameMask = 1 << 4,
        XkbCompatNameMask = 1 << 5,
        XkbKeyTypeNamesMask = 1 << 6,
        XkbKTLevelNamesMask = 1 << 7,
        XkbIndicatorNamesMask = 1 << 8,
        XkbKeyNamesMask = 1 << 9,
        XkbKeyAliasesMask = 1 << 10,
        XkbVirtualModNamesMask = 1 << 11,
        XkbGroupNamesMask = 1 << 12,
        XkbRGNamesMask = 1 << 13,
        XkbComponentNamesMask = 0x3F,
        XkbAllNamesMask = 0x3FFF
    }
}
