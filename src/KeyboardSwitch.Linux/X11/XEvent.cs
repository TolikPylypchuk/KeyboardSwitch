namespace KeyboardSwitch.Linux.X11;

[StructLayout(LayoutKind.Explicit)]
internal struct XEvent
{
    [FieldOffset(0)]
    internal XEventName Type;

    [FieldOffset(0)]
    internal XAnyEvent AnyEvent;

    [FieldOffset(0)]
    internal XKeyEvent KeyEvent;

    [FieldOffset(0)]
    internal XButtonEvent ButtonEvent;

    [FieldOffset(0)]
    internal XMotionEvent MotionEvent;

    [FieldOffset(0)]
    internal XCrossingEvent CrossingEvent;

    [FieldOffset(0)]
    internal XFocusChangeEvent FocusChangeEvent;

    [FieldOffset(0)]
    internal XExposeEvent ExposeEvent;

    [FieldOffset(0)]
    internal XGraphicsExposeEvent GraphicsExposeEvent;

    [FieldOffset(0)]
    internal XNoExposeEvent NoExposeEvent;

    [FieldOffset(0)]
    internal XVisibilityEvent VisibilityEvent;

    [FieldOffset(0)]
    internal XCreateWindowEvent CreateWindowEvent;

    [FieldOffset(0)]
    internal XDestroyWindowEvent DestroyWindowEvent;

    [FieldOffset(0)]
    internal XUnmapEvent UnmapEvent;

    [FieldOffset(0)]
    internal XMapEvent MapEvent;

    [FieldOffset(0)]
    internal XMapRequestEvent MapRequestEvent;

    [FieldOffset(0)]
    internal XReparentEvent ReparentEvent;

    [FieldOffset(0)]
    internal XConfigureEvent ConfigureEvent;

    [FieldOffset(0)]
    internal XGravityEvent GravityEvent;

    [FieldOffset(0)]
    internal XResizeRequestEvent ResizeRequestEvent;

    [FieldOffset(0)]
    internal XConfigureRequestEvent ConfigureRequestEvent;

    [FieldOffset(0)]
    internal XCirculateEvent CirculateEvent;

    [FieldOffset(0)]
    internal XCirculateRequestEvent CirculateRequestEvent;

    [FieldOffset(0)]
    internal XPropertyEvent PropertyEvent;

    [FieldOffset(0)]
    internal XSelectionClearEvent SelectionClearEvent;

    [FieldOffset(0)]
    internal XSelectionRequestEvent SelectionRequestEvent;

    [FieldOffset(0)]
    internal XSelectionEvent SelectionEvent;

    [FieldOffset(0)]
    internal XColormapEvent ColormapEvent;

    [FieldOffset(0)]
    internal XClientMessageEvent ClientMessageEvent;

    [FieldOffset(0)]
    internal XMappingEvent MappingEvent;

    [FieldOffset(0)]
    internal XErrorEvent ErrorEvent;

    [FieldOffset(0)]
    internal XKeymapEvent KeymapEvent;

    [FieldOffset(0)]
    internal XGenericEventCookie GenericEventCookie;

    [FieldOffset(0)]
    internal XEventPad Pad;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XAnyEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XKeyEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal IntPtr Root;
    internal IntPtr Subwindow;
    internal IntPtr Time;
    internal int X;
    internal int Y;
    internal int XRoot;
    internal int YRoot;
    internal XModifierMask State;
    internal int keycode;
    internal int SameScreen;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XButtonEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal IntPtr Root;
    internal IntPtr Subwindow;
    internal IntPtr Time;
    internal int X;
    internal int Y;
    internal int XRoot;
    internal int YRoot;
    internal XModifierMask State;
    internal int Button;
    internal int SameScreen;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XMotionEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal IntPtr Root;
    internal IntPtr Subwindow;
    internal IntPtr Time;
    internal int X;
    internal int Y;
    internal int XRoot;
    internal int YRoot;
    internal XModifierMask State;
    internal byte IsHint;
    internal int SameScreen;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XCrossingEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal IntPtr Root;
    internal IntPtr Subwindow;
    internal IntPtr Time;
    internal int X;
    internal int Y;
    internal int XRoot;
    internal int YRoot;
    internal NotifyMode Mode;
    internal NotifyDetail Detail;
    internal int SameScreen;
    internal int Focus;
    internal XModifierMask State;
}

internal enum NotifyMode
{
    NotifyNormal = 0,
    NotifyGrab = 1,
    NotifyUngrab = 2
}

internal enum NotifyDetail
{
    NotifyAncestor = 0,
    NotifyVirtual = 1,
    NotifyInferior = 2,
    NotifyNonlinear = 3,
    NotifyNonlinearVirtual = 4,
    NotifyPointer = 5,
    NotifyPointerRoot = 6,
    NotifyDetailNone = 7
}

[StructLayout(LayoutKind.Sequential)]
internal struct XFocusChangeEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal int Mode;
    internal NotifyDetail Detail;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XKeymapEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal byte KeyVector0;
    internal byte KeyVector1;
    internal byte KeyVector2;
    internal byte KeyVector3;
    internal byte KeyVector4;
    internal byte KeyVector5;
    internal byte KeyVector6;
    internal byte KeyVector7;
    internal byte KeyVector8;
    internal byte KeyVector9;
    internal byte KeyVector10;
    internal byte KeyVector11;
    internal byte KeyVector12;
    internal byte KeyVector13;
    internal byte KeyVector14;
    internal byte KeyVector15;
    internal byte KeyVector16;
    internal byte KeyVector17;
    internal byte KeyVector18;
    internal byte KeyVector19;
    internal byte KeyVector20;
    internal byte KeyVector21;
    internal byte KeyVector22;
    internal byte KeyVector23;
    internal byte KeyVector24;
    internal byte KeyVector25;
    internal byte KeyVector26;
    internal byte KeyVector27;
    internal byte KeyVector28;
    internal byte KeyVector29;
    internal byte KeyVector30;
    internal byte KeyVector31;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XExposeEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal int X;
    internal int Y;
    internal int Width;
    internal int Height;
    internal int Count;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XGraphicsExposeEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr drawable;
    internal int X;
    internal int Y;
    internal int Width;
    internal int Height;
    internal int Count;
    internal int MajorCode;
    internal int MinorCode;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XNoExposeEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Drawable;
    internal int MajorCode;
    internal int MinorCode;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XVisibilityEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal int State;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XCreateWindowEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Parent;
    internal IntPtr Window;
    internal int X;
    internal int Y;
    internal int Width;
    internal int Height;
    internal int BorderWidth;
    internal int OverrideRedirect;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XDestroyWindowEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr XEvent;
    internal IntPtr Window;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XUnmapEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr XEvent;
    internal IntPtr Window;
    internal int FromConfigure;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XMapEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr XEvent;
    internal IntPtr Window;
    internal int OverrideRedirect;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XMapRequestEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Parent;
    internal IntPtr Window;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XReparentEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr XEvent;
    internal IntPtr Window;
    internal IntPtr Parent;
    internal int X;
    internal int Y;
    internal int OverrideRedirect;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XConfigureEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr XEvent;
    internal IntPtr Window;
    internal int X;
    internal int Y;
    internal int Width;
    internal int Height;
    internal int BorderWidth;
    internal IntPtr Above;
    internal int OverrideRedirect;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XGravityEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr XEvent;
    internal IntPtr Window;
    internal int X;
    internal int Y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XResizeRequestEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal int Width;
    internal int Height;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XConfigureRequestEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Parent;
    internal IntPtr Window;
    internal int X;
    internal int Y;
    internal int Width;
    internal int Height;
    internal int BorderWidth;
    internal IntPtr Above;
    internal int Detail;
    internal IntPtr ValueMask;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XCirculateEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr XEvent;
    internal IntPtr Window;
    internal int Place;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XCirculateRequestEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Parent;
    internal IntPtr Window;
    internal int Place;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XPropertyEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal IntPtr Atom;
    internal IntPtr Time;
    internal int State;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XSelectionClearEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal Atom Selection;
    internal IntPtr Time;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XSelectionRequestEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Owner;
    internal IntPtr Requestor;
    internal Atom Selection;
    internal Atom Target;
    internal Atom Property;
    internal IntPtr Time;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XSelectionEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Requestor;
    internal Atom Selection;
    internal Atom Target;
    internal Atom Property;
    internal IntPtr Time;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XColormapEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal IntPtr Colormap;
    internal int CNew;
    internal int State;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XClientMessageEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal IntPtr MessageType;
    internal int Format;
    internal IntPtr Ptr1;
    internal IntPtr Ptr2;
    internal IntPtr Ptr3;
    internal IntPtr Ptr4;
    internal IntPtr Ptr5;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XMappingEvent
{
    internal XEventName Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal IntPtr Window;
    internal int Request;
    internal int FirstKeycode;
    internal int Count;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XErrorEvent
{
    internal XEventName Type;
    internal IntPtr Display;
    internal IntPtr ResourceId;
    internal IntPtr Serial;
    internal byte ErrorCode;
    internal XRequest RequestCode;
    internal byte MinorCode;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XEventPad
{
    internal IntPtr Pad0;
    internal IntPtr Pad1;
    internal IntPtr Pad2;
    internal IntPtr Pad3;
    internal IntPtr Pad4;
    internal IntPtr Pad5;
    internal IntPtr Pad6;
    internal IntPtr Pad7;
    internal IntPtr Pad8;
    internal IntPtr Pad9;
    internal IntPtr Pad10;
    internal IntPtr Pad11;
    internal IntPtr Pad12;
    internal IntPtr Pad13;
    internal IntPtr Pad14;
    internal IntPtr Pad15;
    internal IntPtr Pad16;
    internal IntPtr Pad17;
    internal IntPtr Pad18;
    internal IntPtr Pad19;
    internal IntPtr Pad20;
    internal IntPtr Pad21;
    internal IntPtr Pad22;
    internal IntPtr Pad23;
    internal IntPtr Pad24;
    internal IntPtr Pad25;
    internal IntPtr Pad26;
    internal IntPtr Pad27;
    internal IntPtr Pad28;
    internal IntPtr Pad29;
    internal IntPtr Pad30;
    internal IntPtr Pad31;
    internal IntPtr Pad32;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct XGenericEventCookie
{
    internal int Type;
    internal IntPtr Serial;
    internal int SendEvent;
    internal IntPtr Display;
    internal int Extension;
    internal int EvType;
    internal uint Cookie;
    internal void* Data;
}
