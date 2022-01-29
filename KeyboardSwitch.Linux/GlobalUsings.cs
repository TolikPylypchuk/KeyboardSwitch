global using System.Collections.Immutable;
global using System.Runtime.InteropServices;

global using KeyboardSwitch.Core;
global using KeyboardSwitch.Core.Keyboard;
global using KeyboardSwitch.Core.Services.AutoConfiguration;
global using KeyboardSwitch.Core.Services.Infrastructure;
global using KeyboardSwitch.Core.Services.InitialSetup;
global using KeyboardSwitch.Core.Services.Layout;
global using KeyboardSwitch.Core.Services.Simulation;
global using KeyboardSwitch.Core.Services.Startup;
global using KeyboardSwitch.Core.Settings;
global using KeyboardSwitch.Linux.Services;
global using KeyboardSwitch.Linux.X11;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using static KeyboardSwitch.Linux.X11.Native;
global using static KeyboardSwitch.Linux.X11.XUtil;
