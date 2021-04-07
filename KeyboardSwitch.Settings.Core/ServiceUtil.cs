using System;

using Splat;

namespace KeyboardSwitch.Settings.Core
{
    public static class ServiceUtil
    {
        public static T GetDefaultService<T>() =>
            Locator.Current.GetService<T>() ??
                throw new InvalidOperationException($"{typeof(T).FullName} not found");
    }
}
