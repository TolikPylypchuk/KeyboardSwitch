namespace KeyboardSwitch.Core;

using System.Runtime.CompilerServices;

using SharpHook.Native;

using RxDisposable = System.Reactive.Disposables.Disposable;

public sealed class BlockingReactiveGlobalHook : IReactiveGlobalHook
{
    private readonly Subject<HookEvent<HookEventArgs>> hookEnabledSubject = new();
    private readonly Subject<HookEvent<HookEventArgs>> hookDisabledSubject = new();

    private readonly Subject<HookEvent<KeyboardHookEventArgs>> keyTypedSubject = new();
    private readonly Subject<HookEvent<KeyboardHookEventArgs>> keyPressedSubject = new();
    private readonly Subject<HookEvent<KeyboardHookEventArgs>> keyReleasedSubject = new();

    private readonly Subject<HookEvent<MouseHookEventArgs>> mouseClickedSubject = new();
    private readonly Subject<HookEvent<MouseHookEventArgs>> mousePressedSubject = new();
    private readonly Subject<HookEvent<MouseHookEventArgs>> mouseReleasedSubject = new();
    private readonly Subject<HookEvent<MouseHookEventArgs>> mouseMovedSubject = new();
    private readonly Subject<HookEvent<MouseHookEventArgs>> mouseDraggedSubject = new();

    private readonly Subject<HookEvent<MouseWheelHookEventArgs>> mouseWheelSubject = new();

    private bool disposed = false;

    public BlockingReactiveGlobalHook()
    {
        this.HookEnabled = this.hookEnabledSubject.AsObservable().Take(1);
        this.HookDisabled = this.hookDisabledSubject.AsObservable().Take(1);

        this.KeyTyped = this.keyTypedSubject.AsObservable();
        this.KeyPressed = this.keyPressedSubject.AsObservable();
        this.KeyReleased = this.keyReleasedSubject.AsObservable();

        this.MouseClicked = this.mouseClickedSubject.AsObservable();
        this.MousePressed = this.mousePressedSubject.AsObservable();
        this.MouseReleased = this.mouseReleasedSubject.AsObservable();
        this.MouseMoved = this.mouseMovedSubject.AsObservable();
        this.MouseDragged = this.mouseDraggedSubject.AsObservable();

        this.MouseWheel = this.mouseWheelSubject.AsObservable();
    }

    ~BlockingReactiveGlobalHook() =>
        this.Dispose(false);

    public bool IsRunning { get; private set; }

    public IObservable<HookEvent<HookEventArgs>> HookEnabled { get; }

    public IObservable<HookEvent<HookEventArgs>> HookDisabled { get; }

    public IObservable<HookEvent<KeyboardHookEventArgs>> KeyTyped { get; }

    public IObservable<HookEvent<KeyboardHookEventArgs>> KeyPressed { get; }

    public IObservable<HookEvent<KeyboardHookEventArgs>> KeyReleased { get; }

    public IObservable<HookEvent<MouseHookEventArgs>> MouseClicked { get; }

    public IObservable<HookEvent<MouseHookEventArgs>> MousePressed { get; }

    public IObservable<HookEvent<MouseHookEventArgs>> MouseReleased { get; }

    public IObservable<HookEvent<MouseHookEventArgs>> MouseMoved { get; }

    public IObservable<HookEvent<MouseHookEventArgs>> MouseDragged { get; }

    public IObservable<HookEvent<MouseWheelHookEventArgs>> MouseWheel { get; }

    public IObservable<Unit> Start()
    {
        this.ThrowIfDisposed();

        return Observable.Create<Unit>(observer =>
        {
            try
            {
                UioHook.SetDispatchProc(this.DispatchEvent);

                this.IsRunning = true;
                var result = UioHook.Run();
                this.IsRunning = false;

                if (result == UioHookResult.Success)
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                } else
                {
                    observer.OnError(new HookException(result, this.FormatFailureMessage("starting", result)));
                }
            } catch (Exception e)
            {
                observer.OnError(e);
            }

            return RxDisposable.Empty;
        });
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void DispatchEvent(ref UioHookEvent e)
    {
        switch (e.Type)
        {
            case EventType.HookEnabled:
                this.hookEnabledSubject.OnNext(new HookEvent<HookEventArgs>(this, new(e)));
                break;
            case EventType.HookDisabled:
                this.hookDisabledSubject.OnNext(new HookEvent<HookEventArgs>(this, new(e)));
                break;
            case EventType.KeyTyped:
                this.keyTypedSubject.OnNext(new HookEvent<KeyboardHookEventArgs>(this, new(e)));
                break;
            case EventType.KeyPressed:
                this.keyPressedSubject.OnNext(new HookEvent<KeyboardHookEventArgs>(this, new(e)));
                break;
            case EventType.KeyReleased:
                this.keyReleasedSubject.OnNext(new HookEvent<KeyboardHookEventArgs>(this, new(e)));
                break;
            case EventType.MouseClicked:
                this.mouseClickedSubject.OnNext(new HookEvent<MouseHookEventArgs>(this, new(e)));
                break;
            case EventType.MousePressed:
                this.mousePressedSubject.OnNext(new HookEvent<MouseHookEventArgs>(this, new(e)));
                break;
            case EventType.MouseReleased:
                this.mouseReleasedSubject.OnNext(new HookEvent<MouseHookEventArgs>(this, new(e)));
                break;
            case EventType.MouseMoved:
                this.mouseMovedSubject.OnNext(new HookEvent<MouseHookEventArgs>(this, new(e)));
                break;
            case EventType.MouseDragged:
                this.mouseDraggedSubject.OnNext(new HookEvent<MouseHookEventArgs>(this, new(e)));
                break;
            case EventType.MouseWheel:
                this.mouseWheelSubject.OnNext(new HookEvent<MouseWheelHookEventArgs>(this, new(e)));
                break;
        };
    }

    private void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        if (this.IsRunning)
        {
            this.hookDisabledSubject.Subscribe(_ =>
            {
                UioHook.SetDispatchProc(UioHook.EmptyDispatchProc);

                this.CompleteAllSubjects();

                if (disposing)
                {
                    this.DisposeAllSubjects();
                }
            });

            var result = UioHook.Stop();

            if (disposing && result != UioHookResult.Success)
            {
                throw new HookException(result, this.FormatFailureMessage("stopping", result));
            }
        }
    }

    private void CompleteAllSubjects()
    {
        this.hookEnabledSubject.OnCompleted();
        this.hookDisabledSubject.OnCompleted();

        this.keyTypedSubject.OnCompleted();
        this.keyPressedSubject.OnCompleted();
        this.keyReleasedSubject.OnCompleted();

        this.mouseClickedSubject.OnCompleted();
        this.mousePressedSubject.OnCompleted();
        this.mouseReleasedSubject.OnCompleted();
        this.mouseMovedSubject.OnCompleted();
        this.mouseDraggedSubject.OnCompleted();

        this.mouseWheelSubject.OnCompleted();
    }

    private void DisposeAllSubjects()
    {
        this.hookEnabledSubject.Dispose();
        this.hookDisabledSubject.Dispose();

        this.keyTypedSubject.Dispose();
        this.keyPressedSubject.Dispose();
        this.keyReleasedSubject.Dispose();

        this.mouseClickedSubject.Dispose();
        this.mousePressedSubject.Dispose();
        this.mouseReleasedSubject.Dispose();
        this.mouseMovedSubject.Dispose();
        this.mouseDraggedSubject.Dispose();

        this.mouseWheelSubject.Dispose();
    }

    private void ThrowIfDisposed([CallerMemberName] string? method = null)
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(
                this.GetType().Name, $"Cannot call {method} - the object is disposed");
        }
    }

    private string FormatFailureMessage(string action, UioHookResult result) =>
        $"Failed {action} the global hook: {result} ({(int)result:x})";
}
