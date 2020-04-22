using System;
using System.Reflection;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services.Infrastructure
{
    internal sealed class SingleInstanceService : ISingleInstanceService
    {
        private readonly INamedPipeService namedPipeService;
        private readonly ILogger<SingleInstanceService> logger;

        public SingleInstanceService(INamedPipeService namedPipeService, ILogger<SingleInstanceService> logger)
        {
            this.namedPipeService = namedPipeService;
            this.logger = logger;
        }

        public Mutex TryAcquireMutex()
        {
            var mutex = new Mutex(false, $"Global\\{Assembly.GetExecutingAssembly().FullName}", out bool createdNew);

            if (!createdNew)
            {
                SendArgumentAndExit();
            }

            bool hasHandle = mutex.WaitOne(5000, false);
            if (!hasHandle)
            {
                const string message = "Timeout waiting for exclusive access on the mutex";
                this.logger.LogError(message);
                throw new TimeoutException(message);
            }

            this.logger.LogTrace("Acquired the global mutex");

            return mutex;
        }

        private void SendArgumentAndExit()
        {
            try
            {
                string message = Environment.GetCommandLineArgs().Length > 1
                    ? Environment.GetCommandLineArgs()[1]
                    : String.Empty;

                this.namedPipeService.Write(message);

                this.logger.LogTrace("Sent the argument to the original instance");
            } catch (Exception e)
            {
                this.logger.LogError(e, "Unknown error during sending an argument to the original instance");
            } finally
            {
                Environment.Exit(0);
            }
        }
    }
}
