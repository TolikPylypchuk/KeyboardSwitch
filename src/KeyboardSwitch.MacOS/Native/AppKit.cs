namespace KeyboardSwitch.MacOS.Native;

internal partial class AppKit
{
    private const string AppKitLib = "/System/Library/Frameworks/AppKit.framework/AppKit";
    private const string MsgSend = "objc_msgSend";

    [LibraryImport(AppKitLib, EntryPoint = "objc_getClass", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr GetClass(string className);

    [LibraryImport(AppKitLib, EntryPoint = MsgSend)]
    public static partial IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [LibraryImport(AppKitLib, EntryPoint = MsgSend, StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr SendMessage(IntPtr receiver, IntPtr selector, string arg1);

    [LibraryImport(AppKitLib, EntryPoint = MsgSend)]
    public static partial IntPtr SendMessage(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [LibraryImport(AppKitLib, EntryPoint = MsgSend)]
    public static partial IntPtr SendMessage(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [LibraryImport(AppKitLib, EntryPoint = "sel_registerName", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr RegisterName(string selectorName);
}
