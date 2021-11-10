global using System.Collections.ObjectModel;
global using System.Globalization;
global using System.Reactive;
global using System.Reactive.Concurrency;
global using System.Reactive.Disposables;
global using System.Reactive.Linq;
global using System.Reactive.Subjects;
global using System.Resources;

global using DynamicData;
global using DynamicData.Aggregation;
global using DynamicData.Binding;

global using KeyboardSwitch.Core;
global using KeyboardSwitch.Core.Keyboard;
global using KeyboardSwitch.Core.Services.AutoConfiguration;
global using KeyboardSwitch.Core.Services.Infrastructure;
global using KeyboardSwitch.Core.Services.Layout;
global using KeyboardSwitch.Core.Services.Settings;
global using KeyboardSwitch.Core.Services.Startup;
global using KeyboardSwitch.Core.Services.Switching;
global using KeyboardSwitch.Core.Services.Text;
global using KeyboardSwitch.Core.Settings;
global using KeyboardSwitch.Settings.Core.Models;
global using KeyboardSwitch.Settings.Core.Services;

global using Microsoft.Extensions.Logging;

global using ReactiveUI;
global using ReactiveUI.Fody.Helpers;
global using ReactiveUI.Validation.Extensions;
global using ReactiveUI.Validation.Helpers;

global using Splat;

global using static KeyboardSwitch.Core.Constants;
global using static KeyboardSwitch.Settings.Core.Constants;
global using static KeyboardSwitch.Settings.Core.ServiceUtil;
