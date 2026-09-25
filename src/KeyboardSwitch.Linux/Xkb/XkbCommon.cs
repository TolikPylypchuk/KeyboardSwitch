namespace KeyboardSwitch.Linux.Xkb;

internal static unsafe partial class XkbCommon
{
    private const string XkbCommonLib = "libxkbcommon.so.0";

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_context_new")]
    public static partial XkbContextHandle ContextNew(XkbContextFlags flags);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_context_unref")]
    public static partial void ContextUnref(IntPtr context);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keymap_new_from_names")]
    public static partial XkbKeymapHandle KeymapNewFromNames(
        XkbContextHandle context,
        in XkbRuleNames names,
        XkbKeymapCompileFlags flags);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keymap_unref")]
    public static partial void KeymapUnref(IntPtr keymap);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keymap_min_keycode")]
    public static partial uint KeymapMinKeycode(XkbKeymapHandle keymap);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keymap_max_keycode")]
    public static partial uint KeymapMaxKeycode(XkbKeymapHandle keymap);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keymap_num_levels_for_key")]
    public static partial uint KeymapNumLevelsForKey(XkbKeymapHandle keymap, uint key, uint layout);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keymap_key_get_syms_by_level")]
    public static partial int KeymapKeyGetSymsByLevel(
        XkbKeymapHandle keymap,
        uint key,
        uint layout,
        uint level,
        out uint* symsOut);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keysym_to_utf32")]
    public static partial uint KeysymToUtf32(uint keysym);
}
