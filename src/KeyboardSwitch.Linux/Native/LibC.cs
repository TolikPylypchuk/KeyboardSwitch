namespace KeyboardSwitch.Linux.Native;

internal static unsafe partial class LibC
{
    public const int EPollIn = 1;
    public const int EPollCtlAdd = 1;
    public const int ONonBlock = 2048;

    private const string C = "libc";

    [LibraryImport(C, EntryPoint = "epoll_create1")]
    public static partial int EPollCreate1(int size);

    [LibraryImport(C, EntryPoint = "epoll_ctl")]
    public static partial int EPollCtl(int epfd, int op, int fd, ref EPollEvent @event);

    [LibraryImport(C, EntryPoint = "epoll_wait")]
    public static partial int EPollWait(int epfd, EPollEvent* events, int maxevents, int timeout);

    [LibraryImport(C, EntryPoint = "pipe2")]
    public static partial int Pipe2(int* fds, int flags);

    [LibraryImport(C, EntryPoint = "write")]
    public static partial nint Write(int fd, void* buf, nint count);

    [LibraryImport(C, EntryPoint = "read")]
    public static partial nint Read(int fd, void* buf, nint count);
}
