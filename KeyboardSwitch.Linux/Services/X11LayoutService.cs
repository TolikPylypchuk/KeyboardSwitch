using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Linux.X11;

using Microsoft.Extensions.Logging;

using X11;

using static KeyboardSwitch.Linux.X11.Native;

namespace KeyboardSwitch.Linux.Services
{
    public sealed class X11LayoutService : ILayoutService
    {
        private static readonly ImmutableList<string> NonSymbols = ImmutableList.Create("group", "inet", "pc");

        private readonly ILogger<X11LayoutService> logger;

        public X11LayoutService(ILogger<X11LayoutService> logger) =>
            this.logger = logger;

        public KeyboardLayout GetCurrentKeyboardLayout()
        {
            this.logger.LogDebug("Getting current keyboard layout");

            using var display = this.GetXDisplay();
            var allLayouts = this.GetKeyboardLayoutsInternal(display);

            XkbSelectEventDetails(
                display,
                XkbUseCoreKbd,
                XkbEventType.XkbStateNotify,
                XStateMask.XkbAllStateComponentsMask,
                XStateMask.XkbGroupStateMask);

            var state = new XkbState();
            XkbGetState(display, XkbUseCoreKbd, ref state);

            return state.Group < allLayouts.Count
                ? allLayouts[state.Group]
                : throw new X11Exception("Current input group is invalid");
        }

        public List<KeyboardLayout> GetKeyboardLayouts()
        {
            this.logger.LogDebug("Getting all keyboard layouts");

            using var display = this.GetXDisplay();
            return this.GetKeyboardLayoutsInternal(display);
        }

        public void SwitchCurrentLayout(SwitchDirection direction)
        { }

        private static KeyboardLayout CreateKeyboardLayout(string group, string symbol, string variant) =>
            new(
                $"{symbol}:{variant}",
                group,
                String.IsNullOrEmpty(variant) ? symbol : $"{symbol} ({variant})",
                String.Empty);

        private static bool IsXkbLayoutSymbol(string symbol) =>
            !NonSymbols.Contains(symbol);

        private XDisplayHandle GetXDisplay()
        {
            this.logger.LogDebug("Getting the X display");

            XkbIgnoreExtension(false);

            int major = XkbMajorVersion;
            int minor = XkbMinorVersion;

            var display = XkbOpenDisplay("", out _, out _, ref major, ref minor, out var result);
            this.ValidateXOpenDisplayResult(result);

            return display;
        }

        private void ValidateXOpenDisplayResult(XOpenDisplayResult result)
        {
            this.logger.LogDebug("Validating opening the X display");

            switch (result)
            {
                case XOpenDisplayResult.BadLibraryVersion:
                    throw new X11Exception("Bad X11 version");
                case XOpenDisplayResult.ConnectionRefused:
                    throw new X11Exception("Connection to X server refused");
                case XOpenDisplayResult.NonXkbServer:
                    throw new X11Exception("XKB not present");
                case XOpenDisplayResult.BadServerVersion:
                    throw new X11Exception("Bad X11 server version");
            }
        }

        private unsafe List<KeyboardLayout> GetKeyboardLayoutsInternal(XDisplayHandle display)
        {
            int major = XkbMajorVersion;
            int minor = XkbMinorVersion;

            XkbQueryExtension(display, out _, out _, out _, ref major, ref minor);

            using var keyboardHandle = XkbAllocKeyboard();
            this.InitKeyboard(display, keyboardHandle);

            var keyboardDesc = (XkbDesc*)keyboardHandle.DangerousGetHandle().ToPointer();

            keyboardDesc->Dpy = display.DangerousGetHandle();

            if (keyboardDesc->Names == IntPtr.Zero)
            {
                this.FreeKeyboard(keyboardHandle);
                throw new X11Exception("Failed to get keyboard description");
            }

            var names = Marshal.PtrToStructure<XkbNames>(keyboardDesc->Names);
            var groupSource = names.Groups;

            int groupCount = this.GetGroupCount(keyboardDesc, groupSource);

            var groupNames = groupSource
                .Where((group, index) => index < groupCount && group != Atom.None)
                .Select(group => this.GetGroupName(display, group))
                .ToList();

            string? symbols = this.GetAllSymbols(display, names.Symbols);

            if (symbols == null)
            {
                this.FreeKeyboard(keyboardHandle);
                throw new X11Exception("Error when getting keyboard layout symbols");
            }

            var (symbolNames, variantNames) = this.ParseSymbols(symbols);

            this.FixUpNames(groupNames, symbolNames);

            this.FreeKeyboard(keyboardHandle);

            return groupNames
                .Zip(symbolNames, (group, symbol) => (Group: group, Symbol: symbol))
                .Zip(variantNames, (gs, variant) => (GroupAndSymbol: gs, Variant: variant))
                .Select(items => CreateKeyboardLayout(
                    items.GroupAndSymbol.Group, items.GroupAndSymbol.Symbol, items.Variant))
                .ToList();
        }

        private string GetGroupName(XDisplayHandle display, Atom group)
        {
            this.logger.LogDebug($"Getting a group name for atom: {group}");

            using var atomNameHandle = XGetAtomName(display, group);
            var atomNameRawHandle = atomNameHandle.DangerousGetHandle();

            return atomNameRawHandle != IntPtr.Zero
                ? Marshal.PtrToStringAuto(atomNameRawHandle) ?? String.Empty
                : String.Empty;
        }

        private string? GetAllSymbols(XDisplayHandle display, Atom symbols)
        {
            this.logger.LogDebug("Getting all symbol names");

            using var symbolsHandle = XGetAtomName(display, symbols);
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
                while (groupCount < XkbNumKbdGroups && groupSource[groupCount] != Atom.None)
                {
                    groupCount++;
                }
            }

            return groupCount != 0 ? groupCount : 1;
        }

        private (List<string> SymbolNames, List<string> VariantNames) ParseSymbols(string symbols)
        {
            this.logger.LogDebug("Parsing keyboard layout symbols");

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
            this.logger.LogDebug($"Finding group #{groupNum}");

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

        private void InitKeyboard(XDisplayHandle display, XHandle keyboardHandle)
        {
            this.logger.LogDebug("Initializing the keyboard");

            XkbGetControls(display, XControlsDetailMask.XkbAllControlsMask, keyboardHandle);
            XkbGetNames(display, XNamesComponentMask.XkbSymbolsNameMask, keyboardHandle);
            XkbGetNames(display, XNamesComponentMask.XkbGroupNamesMask, keyboardHandle);
        }

        private void FreeKeyboard(XHandle keyboardHandle)
        {
            this.logger.LogDebug("Freeing the keyboard");

            XkbFreeControls(keyboardHandle, XControlsDetailMask.XkbAllControlsMask, true);
            XkbFreeNames(keyboardHandle, XNamesComponentMask.XkbSymbolsNameMask, true);
            XkbFreeNames(keyboardHandle, XNamesComponentMask.XkbGroupNamesMask, true);
        }
    }
}
