global using System.Collections.Immutable;
global using System.Reactive;
global using System.Reactive.Concurrency;
global using System.Reactive.Linq;
global using System.Reactive.Subjects;
global using System.Runtime.Serialization;

global using KeyboardSwitch.Core.Keyboard;
global using KeyboardSwitch.Core.Services;
global using KeyboardSwitch.Core.Services.Hook;
global using KeyboardSwitch.Core.Services.Infrastructure;
global using KeyboardSwitch.Core.Services.Layout;
global using KeyboardSwitch.Core.Services.Settings;
global using KeyboardSwitch.Core.Services.Simulation;
global using KeyboardSwitch.Core.Services.Switching;
global using KeyboardSwitch.Core.Services.Text;
global using KeyboardSwitch.Core.Settings;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using SharpHook;
global using SharpHook.Native;
global using SharpHook.Reactive;

global using static KeyboardSwitch.Core.Constants;
global using static KeyboardSwitch.Core.Util;
