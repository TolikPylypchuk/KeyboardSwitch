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

        public SingleInstanceService(
            ServiceResolver<INamedPipeService> namedPipeResolver,
            ILogger<SingleInstanceService> logger,
            string name)
        {
            this.namedPipeService = namedPipeResolver(name);
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
                this.namedPipeService.Write(GetCommand() ?? String.Empty);

                this.logger.LogTrace("Sent the command to the original instance");
            } catch (Exception e)
            {
                this.logger.LogError(e, "Unknown error during sending a command to the original instance");
            } finally
            {
                Environment.Exit(0);
            }
        }

        private string? GetCommand()
        {
            if (Environment.GetCommandLineArgs().Length <= 1)
            {
                return null;
            }

            string arg = Environment.GetCommandLineArgs()[1];

            if (arg.StartsWith("--"))
            {
                return arg[2..];
            }

            if (arg.StartsWith('-') || arg.StartsWith('/'))
            {
                return arg[1..];
            }

            return arg;
        }
    }
}
