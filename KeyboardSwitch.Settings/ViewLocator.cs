using Avalonia.Controls;
using Avalonia.Controls.Templates;

using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling
            => false;

        public IControl Build(object data)
        {
            var view = Locator.Current.GetService(typeof(IViewFor<>).MakeGenericType(data.GetType()));

            return view is IControl control
                ? control
                : new TextBlock { Text = "Not Found: " + view.GetType().FullName };
        }

        public bool Match(object data)
            => data is ReactiveObject;
    }
}
