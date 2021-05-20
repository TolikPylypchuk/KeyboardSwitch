using System;
using System.Reflection;
using System.Runtime.InteropServices;

using static KeyboardSwitch.Core.Util;

namespace KeyboardSwitch.Core.Hook
{
    public delegate void DispatchProc(ref UioHookEvent e);

    public delegate bool LogProc(uint level, string message);

    internal static class UioHook
    {
        public const string LibUioHook = "uiohook";

        static UioHook() =>
            NativeLibrary.SetDllImportResolver(typeof(UioHook).Assembly, ImportResolver);

        [DllImport(LibUioHook, EntryPoint = "hook_set_dispatch_proc")]
        internal static extern void SetDispatchProc(DispatchProc dispatchProc);

        [DllImport(LibUioHook, EntryPoint = "hook_run")]
        internal static extern UioHookResult Run();

        [DllImport(LibUioHook, EntryPoint = "hook_stop")]
        internal static extern UioHookResult Stop();

        [DllImport(LibUioHook, EntryPoint = "hook_create_screen_info")]
        internal static extern ScreenData[] CreateScreenInfo(byte[] count);

        [DllImport(LibUioHook, EntryPoint = "hook_get_auto_repeat_rate")]
        internal static extern long GetAutoRepeatRate();

        [DllImport(LibUioHook, EntryPoint = "hook_get_auto_repeat_delay")]
        internal static extern long GetAutoRepeatDelay();

        [DllImport(LibUioHook, EntryPoint = "hook_get_pointer_acceleration_multiplier")]
        internal static extern long GetPointerAccelerationMultiplier();

        [DllImport(LibUioHook, EntryPoint = "hook_get_pointer_acceleration_threshold")]
        internal static extern long GetPointerAccelerationThreshold();

        [DllImport(LibUioHook, EntryPoint = "hook_get_pointer_sensitivity")]
        internal static extern long GetPointerSensitivity();

        [DllImport(LibUioHook, EntryPoint = "hook_get_multi_click_time")]
        internal static extern long GetMultiClickTime();

        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var libHandle = IntPtr.Zero;

            if (libraryName == LibUioHook)
            {
                NativeLibrary.TryLoad(
                    GetLibUioHookName(), assembly, DllImportSearchPath.AssemblyDirectory, out libHandle);
            }

            return libHandle;
        }

        private static string GetLibUioHookName() =>
            PlatformDependent(
                windows: () => @".\uiohook.dll",
                macos: () => "./libuiohook.dylib",
                linux: () => "./libuiohook.so");
    }
}
