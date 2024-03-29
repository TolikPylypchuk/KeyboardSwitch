global using System.Collections.Immutable;
global using System.Diagnostics;
global using System.Runtime.InteropServices;
global using System.Text;

global using KeyboardSwitch.Core;
global using KeyboardSwitch.Core.Keyboard;
global using KeyboardSwitch.Core.Services.AutoConfiguration;
global using KeyboardSwitch.Core.Services.Clipboard;
global using KeyboardSwitch.Core.Services.Infrastructure;
global using KeyboardSwitch.Core.Services.InitialSetup;
global using KeyboardSwitch.Core.Services.Layout;
global using KeyboardSwitch.Core.Services.Simulation;
global using KeyboardSwitch.Core.Services.Startup;
global using KeyboardSwitch.Core.Services.Users;
global using KeyboardSwitch.Core.Settings;
global using KeyboardSwitch.MacOS.Native;
global using KeyboardSwitch.MacOS.Services;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using static KeyboardSwitch.MacOS.LaunchdConstants;
global using static KeyboardSwitch.MacOS.Native.NativeUtils;
