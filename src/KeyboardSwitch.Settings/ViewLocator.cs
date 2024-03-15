using Avalonia.Controls.Templates;

using Splat;

namespace KeyboardSwitch.Settings;

public sealed class ViewLocator : IDataTemplate
{
    public bool SupportsRecycling => false;

    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var view = Locator.Current.GetService(typeof(IViewFor<>).MakeGenericType(data.GetType()));

        return view is Control control
            ? control
            : new TextBlock { Text = "Not Found: " + view?.GetType().FullName };
    }

    public bool Match(object? data) =>
        data is ReactiveObject;
}
