namespace KeyboardSwitch.Linux.Services;

using System.Runtime.InteropServices;
using System.Text;

internal class XLayoutService(X11Service x11, ILogger<XLayoutService> logger) : CachingLayoutService
{
    private static readonly ImmutableList<string> NonSymbols = ["group", "inet", "pc"];

    public override KeyboardLayout GetCurrentKeyboardLayout()
    {
        logger.LogDebug("Getting current keyboard layout");

        var allLayouts = this.GetKeyboardLayouts();

        XLib.XkbSelectEventDetails(
            x11.Display,
            XkbKeyboardSpec.XkbUseCoreKbd,
            XkbEventType.XkbStateNotify,
            XStateMask.XkbAllStateComponentsMask,
            XStateMask.XkbGroupStateMask);

        var state = new XkbState();
        XLib.XkbGetState(x11.Display, XkbKeyboardSpec.XkbUseCoreKbd, ref state);

        return state.Group < allLayouts.Count
            ? allLayouts[state.Group]
            : throw new XException("Current input group is invalid");
    }

    public override void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    {
        logger.LogDebug("Switching the current layout {Direction}", direction.AsString());

        var allLayouts = this.GetKeyboardLayouts();

        XLib.XkbSelectEventDetails(
            x11.Display,
            XkbKeyboardSpec.XkbUseCoreKbd,
            XkbEventType.XkbStateNotify,
            XStateMask.XkbAllStateComponentsMask,
            XStateMask.XkbGroupStateMask);

        var state = new XkbState();
        XLib.XkbGetState(x11.Display, XkbKeyboardSpec.XkbUseCoreKbd, ref state);

        int offset = direction == SwitchDirection.Forward ? 1 : -1;
        int newGroup = (state.Group + offset + allLayouts.Count) % allLayouts.Count;

        this.SetLayout((uint)newGroup);
    }

    protected override unsafe List<KeyboardLayout> GetKeyboardLayoutsInternal()
    {
        logger.LogDebug("Getting all keyboard layouts");

        int major = XLib.XkbMajorVersion;
        int minor = XLib.XkbMinorVersion;

        XLib.XkbQueryExtension(x11.Display, out _, out _, out _, ref major, ref minor);

        using var keyboardHandle = XLib.XkbAllocKeyboard();
        this.InitKeyboard(keyboardHandle);

        var keyboardDesc = (XkbDesc*)keyboardHandle.DangerousGetHandle().ToPointer();

        if (keyboardDesc->Names == IntPtr.Zero)
        {
            this.FreeKeyboard(keyboardHandle);
            throw new XException("Failed to get keyboard description");
        }

        keyboardDesc->Display = x11.Display.DangerousGetHandle();

        var names = Marshal.PtrToStructure<XkbNames>(keyboardDesc->Names);

        int groupCount = this.GetGroupCount(keyboardDesc, names.Groups);

        var groupNames = names.Groups
            .Where((group, index) => index < groupCount && group != Atom.None)
            .Select(this.GetGroupName)
            .ToList();

        string? symbols = this.GetAllSymbols(names.Symbols);

        if (symbols == null)
        {
            this.FreeKeyboard(keyboardHandle);
            throw new XException("Error when getting keyboard layout symbols");
        }

        var (symbolNames, variantNames) = this.ParseSymbols(symbols);

        this.FixUpNames(groupNames, symbolNames);

        this.FreeKeyboard(keyboardHandle);

        return Enumerable.Zip(groupNames, symbolNames, variantNames)
            .Select(items => CreateKeyboardLayout(items.First, items.Second, items.Third))
            .Distinct()
            .ToList();
    }

    private protected virtual void SetLayout(uint group) =>
        XLib.XkbLockGroup(x11.Display, XkbKeyboardSpec.XkbUseCoreKbd, group);

    private static KeyboardLayout CreateKeyboardLayout(string group, string symbol, string variant) =>
        new(
            $"{symbol}:{variant}",
            group,
            String.IsNullOrEmpty(variant) ? symbol : $"{symbol} ({variant})",
            String.Empty);

    private static bool IsXkbLayoutSymbol(string symbol) =>
        !NonSymbols.Contains(symbol);

    private string GetGroupName(Atom group)
    {
        logger.LogDebug("Getting a group name for atom: {Group}", group);

        using var atomNameHandle = XLib.XGetAtomName(x11.Display, group);
        var atomNameRawHandle = atomNameHandle.DangerousGetHandle();

        return atomNameRawHandle != IntPtr.Zero
            ? Marshal.PtrToStringAuto(atomNameRawHandle) ?? String.Empty
            : String.Empty;
    }

    private string? GetAllSymbols(Atom symbols)
    {
        logger.LogDebug("Getting all symbol names");

        using var symbolsHandle = XLib.XGetAtomName(x11.Display, symbols);
        var symbolsRawHandle = symbolsHandle.DangerousGetHandle();

        return symbolsRawHandle != IntPtr.Zero
            ? Marshal.PtrToStringAuto(symbolsRawHandle)
            : null;
    }

    private void FixUpNames(List<string> groupNames, List<string> symbolNames)
    {
        while (symbolNames.Count < groupNames.Count)
        {
            symbolNames.Insert(0, "us");
        }

        for (int i = 0; i < groupNames.Count; i++)
        {
            if (String.IsNullOrEmpty(groupNames[i]))
            {
                var name = this.GetSymbolName(groupNames, symbolNames, i);
                if (String.IsNullOrEmpty(name))
                {
                    name = "N/A";
                }

                groupNames[i] = name;
            }
        }
    }

    private unsafe int GetGroupCount(XkbDesc* keyboardDesc, Atom[] groupSource)
    {
        int groupCount = 0;

        if (keyboardDesc->Ctrls != IntPtr.Zero)
        {
            groupCount = Marshal.PtrToStructure<XkbControls>(keyboardDesc->Ctrls).NumGroups;
        } else
        {
            while (groupCount < XLib.XkbNumKbdGroups && groupSource[groupCount] != Atom.None)
            {
                groupCount++;
            }
        }

        return groupCount != 0 ? groupCount : 1;
    }

    private (List<string> SymbolNames, List<string> VariantNames) ParseSymbols(string symbols)
    {
        logger.LogDebug("Parsing keyboard layout symbols");

        bool inSymbol = false;
        var currentSymbol = new StringBuilder();
        var currentVariant = new StringBuilder();

        var symbolNames = new List<string>();
        var variantNames = new List<string>();

        for (int i = 0; i < symbols.Length; i++)
        {
            char ch = symbols[i];
            if (ch == '+' || ch == '_')
            {
                if (inSymbol)
                {
                    if (IsXkbLayoutSymbol(currentSymbol.ToString()))
                    {
                        symbolNames.Add(currentSymbol.ToString());
                        variantNames.Add(currentVariant.ToString());
                    }

                    currentSymbol.Clear();
                    currentVariant.Clear();
                } else
                {
                    inSymbol = true;
                }
            } else if (inSymbol && (Char.IsLetter(ch) || ch == '_'))
            {
                currentSymbol.Append(ch);
            } else if (inSymbol && ch == '(')
            {
                while (++i < symbols.Length && symbols[i] != ')')
                {
                    currentVariant.Append(symbols[i]);
                }
            } else
            {
                if (inSymbol)
                {
                    if (IsXkbLayoutSymbol(currentSymbol.ToString()))
                    {
                        symbolNames.Add(currentSymbol.ToString());
                        variantNames.Add(currentVariant.ToString());
                    }

                    currentSymbol.Clear();
                    currentVariant.Clear();
                    inSymbol = false;
                }
            }
        }

        if (inSymbol && currentSymbol.Length != 0 && IsXkbLayoutSymbol(currentSymbol.ToString()))
        {
            symbolNames.Add(currentSymbol.ToString());
            variantNames.Add(currentVariant.ToString());
        }

        return (symbolNames, variantNames);
    }

    private string GetSymbolName(List<string> groupNames, List<string> symbolNames, int groupNum) =>
        symbolNames[this.FindGroup(groupNames, symbolNames, groupNum)];

    private int FindGroup(List<string> groupNames, List<string> symbolNames, int groupNum)
    {
        logger.LogDebug("Finding group #{GroupNumber}", groupNum);

        string sourceText = groupNames[groupNum];
        int result = groupNum;

        if (sourceText != null)
        {
            result = symbolNames
                .Select((name, i) => (Name: name, Index: i))
                .Where(name => name.Name.Equals(sourceText, StringComparison.InvariantCultureIgnoreCase))
                .Select(name => (int?)name.Index)
                .FirstOrDefault()
                ?? groupNum;
        }

        return result;
    }

    private void InitKeyboard(XHandle keyboardHandle)
    {
        logger.LogDebug("Initializing the keyboard");

        XLib.XkbGetControls(x11.Display, XControlsDetailMask.XkbAllControlsMask, keyboardHandle);
        XLib.XkbGetNames(x11.Display, XNamesComponentMask.XkbSymbolsNameMask, keyboardHandle);
        XLib.XkbGetNames(x11.Display, XNamesComponentMask.XkbGroupNamesMask, keyboardHandle);
    }

    private void FreeKeyboard(XHandle keyboardHandle)
    {
        logger.LogDebug("Freeing the keyboard");

        XLib.XkbFreeControls(keyboardHandle, XControlsDetailMask.XkbAllControlsMask, true);
        XLib.XkbFreeNames(keyboardHandle, XNamesComponentMask.XkbSymbolsNameMask, true);
        XLib.XkbFreeNames(keyboardHandle, XNamesComponentMask.XkbGroupNamesMask, true);
    }
}
