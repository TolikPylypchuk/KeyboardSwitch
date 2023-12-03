namespace KeyboardSwitch.Core.Keyboard;

public sealed class DisposableLayouts(IEnumerable<KeyboardLayout> layouts, IDisposable layoutDisposable) : Disposable
{
    private readonly IDisposable layoutDisposable = layoutDisposable;

    public IReadOnlyList<KeyboardLayout> Layouts { get; } = new List<KeyboardLayout>(layouts).AsReadOnly();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.layoutDisposable.Dispose();
        }
    }
}
