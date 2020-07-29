// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings.Infrastructure
{
    /// <summary>
    /// Extension methods associated with the ISuspensionHost interface.
    /// </summary>
    public static class SuspensionHostExtensions
    {
        /// <summary>
        /// Observe changes to the AppState of a class derived from ISuspensionHost.
        /// </summary>
        /// <typeparam name="T">The observable type.</typeparam>
        /// <param name="item">The suspension host.</param>
        /// <returns>An observable of the app state.</returns>
        public static IObservable<T> ObserveAppStateCorrect<T>(this ISuspensionHost item)
            where T : class
        {
            return item.WhenAny(x => x.AppState, x => (T)x.Value!)
                        .Where(x => x != null);
        }

        /// <summary>
        /// Get the current App State of a class derived from ISuspensionHost.
        /// </summary>
        /// <typeparam name="T">The app state type.</typeparam>
        /// <param name="item">The suspension host.</param>
        /// <returns>The app state.</returns>
        public static T GetAppStateCorrect<T>(this ISuspensionHost item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.AppState == null)
            {
                throw new NullReferenceException(nameof(item.AppState));
            }

            return (T)item.AppState;
        }

        /// <summary>
        /// Setup our suspension driver for a class derived off ISuspensionHost interface.
        /// This will make your suspension host respond to suspend and resume requests.
        /// </summary>
        /// <param name="item">The suspension host.</param>
        /// <param name="driver">The suspension driver.</param>
        /// <returns>A disposable which will stop responding to Suspend and Resume requests.</returns>
        public static IDisposable SetupDefaultSuspendResumeCorrect(this ISuspensionHost item, ISuspensionDriver? driver = null)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var ret = new CompositeDisposable();
            driver ??= Locator.Current.GetService<ISuspensionDriver>();

            ret.Add(item.ShouldInvalidateState
                         .SelectMany(_ => driver.InvalidateState())
                         .LoggedCatch(item, Observable.Return(Unit.Default), "Tried to invalidate app state")
                         .Subscribe(_ => item.Log().Info("Invalidated app state")));

            ret.Add(item.ShouldPersistState
                         .SelectMany(x => driver.SaveState(item.AppState!).Finally(x.Dispose))
                         .LoggedCatch(item, Observable.Return(Unit.Default), "Tried to persist app state")
                         .Subscribe(_ => item.Log().Info("Persisted application state")));

            ret.Add(Observable.Merge(item.IsResuming, item.IsLaunchingNew)
                              .SelectMany(x => driver.LoadState())
                              .LoggedCatch(
                                  item,
                                  Observable.Defer(() => Observable.Return(item.CreateNewAppState?.Invoke())),
                                  "Failed to restore app state from storage, creating from scratch")
                                  .Subscribe(x => item.AppState = x ?? item.CreateNewAppState?.Invoke()));

            return ret;
        }
    }
}
