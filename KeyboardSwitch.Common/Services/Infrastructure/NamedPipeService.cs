using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services.Infrastructure
{
    internal sealed class NamedPipeService : INamedPipeService
    {
        private readonly Subject<string> receivedString = new Subject<string>();
        private readonly ILogger<NamedPipeService> logger;

        public NamedPipeService(ILogger<NamedPipeService> logger)
        {
            this.logger = logger;
            this.NamedPipeName = nameof(KeyboardSwitch);
        }

        public string NamedPipeName { get; }

        public IObservable<string> ReceivedString
            => this.receivedString.AsObservable();

        public void StartServer()
            => Task.Run(this.WaitForMessages);

        public bool Write(string text, int connectTimeout = 300)
        {
            using var client = new NamedPipeClientStream(this.NamedPipeName);

            try
            {
                client.Connect(connectTimeout);
            } catch (Exception e)
            {
                this.logger.LogError(e, "Named pipe error");
                return false;
            }

            if (!client.IsConnected)
            {
                this.logger.LogError("The client is not connected");
                return false;
            }

            using var writer = new StreamWriter(client);
            writer.Write(text);
            writer.Flush();

            return true;
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        private void WaitForMessages()
        {
            while (true)
            {
                string text;
                using (var server = new NamedPipeServerStream(this.NamedPipeName, PipeDirection.InOut, 10))
                {
                    server.WaitForConnection();

                    using var reader = new StreamReader(server);
                    text = reader.ReadToEnd();
                }

                this.receivedString.OnNext(text);
            }
        }
    }
}
