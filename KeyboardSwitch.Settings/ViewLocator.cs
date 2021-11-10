namespace KeyboardSwitch.Settings;

using Avalonia.Controls.Templates;

using Splat;

public sealed class ViewLocator : IDataTemplate
{
    public bool SupportsRecycling => false;

    public IControl Build(object data)
    {
        var view = Locator.Current.GetService(typeof(IViewFor<>).MakeGenericType(data.GetType()));

        return view is IControl control
            ? control
            : new TextBlock { Text = "Not Found: " + view?.GetType().FullName };
    }

    public bool Match(object data) =>
        data is ReactiveObject;
}
