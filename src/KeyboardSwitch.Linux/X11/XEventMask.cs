namespace KeyboardSwitch.Linux.X11;

[Flags]
internal enum XEventMask
{
    NoEventMask = 0,
    KeyPressMask = 1 << 0,
    KeyReleaseMask = 1 << 1,
    ButtonPressMask = 1 << 2,
    ButtonReleaseMask = 1 << 3,
    EnterWindowMask = 1 << 4,
    LeaveWindowMask = 1 << 5,
    PointerMotionMask = 1 << 6,
    PointerMotionHintMask = 1 << 7,
    Button1MotionMask = 1 << 8,
    Button2MotionMask = 1 << 9,
    Button3MotionMask = 1 << 10,
    Button4MotionMask = 1 << 11,
    Button5MotionMask = 1 << 12,
    ButtonMotionMask = 1 << 13,
    KeymapStateMask = 1 << 14,
    ExposureMask = 1 << 15,
    VisibilityChangeMask = 1 << 16,
    StructureNotifyMask = 1 << 17,
    ResizeRedirectMask = 1 << 18,
    SubstructureNotifyMask = 1 << 19,
    SubstructureRedirectMask = 1 << 20,
    FocusChangeMask = 1 << 21,
    PropertyChangeMask = 1 << 22,
    ColormapChangeMask = 1 << 23,
    OwnerGrabButtonMask = 1 << 24
}
