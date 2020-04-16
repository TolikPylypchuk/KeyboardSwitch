using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeyboardSwitch.Common
{
    public static class TaskUtils
    {
        public static Task RunSTATask(Action action)
        {
            var source = new TaskCompletionSource<object?>();

            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    source.SetResult(null);
                } catch (Exception e)
                {
                    source.SetException(e);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return source.Task;
        }

        public static Task<T> RunSTATask<T>(Func<T> func)
        {
            var source = new TaskCompletionSource<T>();

            var thread = new Thread(() =>
            {
                try
                {
                    source.SetResult(func());
                } catch (Exception e)
                {
                    source.SetException(e);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return source.Task;
        }
    }
}
