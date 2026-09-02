namespace KeyboardSwitch.Linux.Services;

internal unsafe sealed partial class XMainLoopRunner(X11Service x11, ILogger<XMainLoopRunner> logger) : IMainLoopRunner
{
    private const int EPollTimeout = 1000;

    public void RunMainLoop(CancellationToken token)
    {
        var (epoll, eventFd) = this.InitializeEPoll();

        this.LogRunningMainLoop();

        using var registration = token.Register(() =>
        {
            ulong signal = 1;
            LibC.Write(eventFd, &signal, 8);
        });

        while (!token.IsCancellationRequested)
        {
            XLib.XFlush(x11.Display);

            EPollEvent ev;

            if (XLib.XPending(x11.Display) == 0)
            {
                LibC.EPollWait(epoll, &ev, 1, EPollTimeout);

                ulong buf = 0;
                LibC.Read(eventFd, &buf, 8);
            }

            if (!token.IsCancellationRequested)
            {
                this.HandleX11Events(token);
            }
        }

        LibC.Close(epoll);
        LibC.Close(eventFd);
    }

    private (int EPoll, int EventFd) InitializeEPoll()
    {
        this.LogCreatingEpollConnection();

        int epoll = LibC.EPollCreate1(0);

        if (epoll == -1)
        {
            throw new XException("epoll_create1 failed");
        }

        var fd = XLib.XConnectionNumber(x11.Display);

        var ev = new EPollEvent()
        {
            Events = LibC.EPollIn,
            Data = { U32 = (int)EventCode.X11 }
        };

        if (LibC.EPollCtl(epoll, LibC.EPollCtlAdd, fd, ref ev) == -1)
        {
            throw new XException("Unable to attach X11 connection handle to epoll");
        }

        int eventFd = LibC.EventFd(0, LibC.EFDNonBlock);

        if (eventFd == -1)
        {
            throw new XException("eventfd failed");
        }

        ev = new EPollEvent
        {
            Events = LibC.EPollIn,
            Data = { U32 = (int)EventCode.Signal }
        };

        if (LibC.EPollCtl(epoll, LibC.EPollCtlAdd, eventFd, ref ev) == -1)
        {
            throw new XException("Unable to attach signal pipe to epoll");
        }

        return (epoll, eventFd);
    }

    private void HandleX11Events(CancellationToken token)
    {
        while (XLib.XPending(x11.Display) != 0)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            XLib.XNextEvent(x11.Display, out var xEvent);
            if (XLib.XFilterEvent(ref xEvent, IntPtr.Zero))
            {
                continue;
            }

            if (xEvent.Type == XEventName.GenericEvent)
            {
                XLib.XGetEventData(x11.Display, &xEvent.GenericEventCookie);
            }

            try
            {
                if (x11.TryGetEventHandler(xEvent.AnyEvent.Window, out var handler))
                {
                    this.LogHandlingEvent();
                    handler(ref xEvent);
                }
            } finally
            {
                if (xEvent.Type == XEventName.GenericEvent && xEvent.GenericEventCookie.Data != null)
                {
                    XLib.XFreeEventData(x11.Display, &xEvent.GenericEventCookie);
                }
            }
        }
    }

    [LoggerMessage(LogLevel.Information, "Running the main loop to listen for X11 events")]
    private partial void LogRunningMainLoop();

    [LoggerMessage(LogLevel.Information, "Creating an epoll connection to listen for X11 events")]
    private partial void LogCreatingEpollConnection();

    [LoggerMessage(LogLevel.Debug, "Handling an event from X11")]
    private partial void LogHandlingEvent();
}
