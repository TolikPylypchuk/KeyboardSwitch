global using System.Collections.Immutable;
global using System.Globalization;
global using System.Reactive;
global using System.Reactive.Disposables;
global using System.Reactive.Linq;

global using Avalonia;
global using Avalonia.Controls;
global using Avalonia.Controls.Primitives;
global using Avalonia.Input;
global using Avalonia.Interactivity;
global using Avalonia.Markup.Xaml;
global using Avalonia.ReactiveUI;

global using DynamicData;
global using DynamicData.Aggregation;
global using DynamicData.Binding;

global using KeyboardSwitch.Core;
global using KeyboardSwitch.Core.Services;
global using KeyboardSwitch.Core.Services.Infrastructure;
global using KeyboardSwitch.Core.Services.InitialSetup;
global using KeyboardSwitch.Core.Services.Settings;
global using KeyboardSwitch.Core.Settings;
global using KeyboardSwitch.Settings.Converters;
global using KeyboardSwitch.Settings.Core.ViewModels;
global using KeyboardSwitch.Settings.Properties;
global using KeyboardSwitch.Settings.State;
global using KeyboardSwitch.Settings.Views;

global using ReactiveUI;
global using ReactiveUI.Validation.Extensions;

global using SharpHook.Native;

global using static KeyboardSwitch.Core.Util;
global using static KeyboardSwitch.Settings.Core.Constants;
global using static KeyboardSwitch.Settings.Core.ServiceUtil;
