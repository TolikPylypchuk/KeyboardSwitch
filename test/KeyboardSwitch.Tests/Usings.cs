global using System.Collections.Immutable;
global using System.Reactive;

global using FsCheck;
global using FsCheck.Fluent;
global using FsCheck.Xunit;

global using KeyboardSwitch.Core;
global using KeyboardSwitch.Core.Exceptions;
global using KeyboardSwitch.Core.Keyboard;
global using KeyboardSwitch.Core.Services.AutoConfiguration;
global using KeyboardSwitch.Core.Services.Clipboard;
global using KeyboardSwitch.Core.Services.Hook;
global using KeyboardSwitch.Core.Services.Layout;
global using KeyboardSwitch.Core.Services.Settings;
global using KeyboardSwitch.Core.Services.Simulation;
global using KeyboardSwitch.Core.Services.Switching;
global using KeyboardSwitch.Core.Settings;
global using KeyboardSwitch.Tests.Data;
global using KeyboardSwitch.Tests.Logging;

global using Microsoft.Extensions.Logging;
global using Microsoft.Reactive.Testing;

global using NSubstitute;

global using SharpHook.Data;
