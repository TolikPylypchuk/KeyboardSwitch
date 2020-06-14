using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeyboardSwitch.Common
{
    public sealed class TaskQueue : Disposable
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            this.ThrowIfDisposed();

            await semaphore.WaitAsync();

            try
            {
                return await taskGenerator();
            } finally
            {
                semaphore.Release();
            }
        }

        public async Task Enqueue(Func<Task> taskGenerator)
        {
            this.ThrowIfDisposed();

            await semaphore.WaitAsync();

            try
            {
                await taskGenerator();
            } finally
            {
                semaphore.Release();
            }
        }

        public async void EnqueueAndIgnore(Func<Task> taskGenerator)
        {
            this.ThrowIfDisposed();

            await semaphore.WaitAsync();

            try
            {
                await taskGenerator();
            } finally
            {
                semaphore.Release();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.semaphore.Dispose();
            }
        }
    }
}
