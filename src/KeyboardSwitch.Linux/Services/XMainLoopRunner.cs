namespace KeyboardSwitch.Linux.Services;

internal unsafe sealed class XMainLoopRunner : IMainLoopRunner
{
    private const int EPollTimeout = 1000;

    private readonly X11Service x11;
    private readonly ILogger<XMainLoopRunner> logger;

    private readonly int epoll;
    private readonly int sigread;
    private readonly int sigwrite;

    public XMainLoopRunner(X11Service x11, ILogger<XMainLoopRunner> logger)
    {
        this.x11 = x11;
        this.logger = logger;

        this.epoll = LibC.EPollCreate1(0);

        if (this.epoll == -1)
        {
            throw new XException("epoll_create1 failed");
        }

        var fd = XLib.XConnectionNumber(x11.Display);
        var ev = new EPollEvent()
        {
            Events = LibC.EPollIn,
            Data = { U32 = (int)EventCode.X11 }
        };

        if (LibC.EPollCtl(this.epoll, LibC.EPollCtlAdd, fd, ref ev) == -1)
        {
            throw new XException("Unable to attach X11 connection handle to epoll");
        }

        var fds = stackalloc int[2];
        LibC.Pipe2(fds, LibC.ONonBlock);
        this.sigread = fds[0];
        this.sigwrite = fds[1];

        ev = new EPollEvent
        {
            Events = LibC.EPollIn,
            Data = { U32 = (int)EventCode.Signal }
        };

        if (LibC.EPollCtl(this.epoll, LibC.EPollCtlAdd, this.sigread, ref ev) == -1)
        {
            throw new XException("Unable to attach signal pipe to epoll");
        }
    }

    public void RunMainLoop(CancellationToken token)
    {
        this.logger.LogInformation("Running the main loop to listen for X11 events");

        while (!token.IsCancellationRequested)
        {
            XLib.XFlush(this.x11.Display);

            EPollEvent ev;

            if (XLib.XPending(this.x11.Display) == 0)
            {
                LibC.EPollWait(this.epoll, &ev, 1, EPollTimeout);

                int buf = 0;
                while (LibC.Read(this.sigread, &buf, 4) > 0)
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

    private unsafe void HandleX11Events(CancellationToken token)
    {
        while (XLib.XPending(this.x11.Display) != 0)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            XLib.XNextEvent(this.x11.Display, out var xEvent);
            if (XLib.XFilterEvent(ref xEvent, IntPtr.Zero))
            {
                continue;
            }

            if (xEvent.Type == XEventName.GenericEvent)
            {
                XLib.XGetEventData(this.x11.Display, &xEvent.GenericEventCookie);
            }

            try
            {
                if (this.x11.TryGetEventHandler(xEvent.AnyEvent.Window, out var handler))
                {
                    this.logger.LogDebug("Handling an event from X11");
                    handler(ref xEvent);
                }
            } finally
            {
                if (xEvent.Type == XEventName.GenericEvent && xEvent.GenericEventCookie.Data != null)
                {
                    XLib.XFreeEventData(this.x11.Display, &xEvent.GenericEventCookie);
                }
            }
        }
    }
}
