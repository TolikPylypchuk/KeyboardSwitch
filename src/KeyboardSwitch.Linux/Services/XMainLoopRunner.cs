namespace KeyboardSwitch.Linux.Services;

internal unsafe sealed class XMainLoopRunner(X11Service x11, ILogger<XMainLoopRunner> logger) : IMainLoopRunner
{
    private const int EPollTimeout = 1000;

    public void RunMainLoop(CancellationToken token)
    {
        var (epoll, sigread) = this.InitializeEPoll();

        logger.LogInformation("Running the main loop to listen for X11 events");

        while (!token.IsCancellationRequested)
        {
            XLib.XFlush(x11.Display);

            EPollEvent ev;

            if (XLib.XPending(x11.Display) == 0)
            {
                LibC.EPollWait(epoll, &ev, 1, EPollTimeout);

                int buf = 0;

                while (LibC.Read(sigread, &buf, 4) > 0)
                {
                    continue;
                }
            }

            if (!token.IsCancellationRequested)
            {
                this.HandleX11Events(token);
            }
        }
    }

    private (int EPoll, int SigRead) InitializeEPoll()
    {
        logger.LogInformation("Creating an epoll connection to listen for X11 events");

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

        var fds = stackalloc int[2];
        LibC.Pipe2(fds, LibC.ONonBlock);
        int sigread = fds[0];

        ev = new EPollEvent
        {
            Events = LibC.EPollIn,
            Data = { U32 = (int)EventCode.Signal }
        };

        if (LibC.EPollCtl(epoll, LibC.EPollCtlAdd, sigread, ref ev) == -1)
        {
            throw new XException("Unable to attach signal pipe to epoll");
        }

        return (epoll, sigread);
    }

    private unsafe void HandleX11Events(CancellationToken token)
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
                    logger.LogDebug("Handling an event from X11");
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
}
