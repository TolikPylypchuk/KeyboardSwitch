using Avalonia.Controls.Templates;

namespace KeyboardSwitch.Settings;

public sealed class ViewLocatorDataTemplate : IDataTemplate
{
    public bool SupportsRecycling => false;

    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var view = ViewLocator.GetCurrent().ResolveView(data, null);

        return view is Control control
            ? control
            : new TextBlock { Text = "Not Found: " + view?.GetType().FullName };
    }

    public bool Match(object? data) =>
        data is ReactiveObject;
}
