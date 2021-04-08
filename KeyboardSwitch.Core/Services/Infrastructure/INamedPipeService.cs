using System;

namespace KeyboardSwitch.Core.Services.Infrastructure
{
    public interface INamedPipeService
    {
        string NamedPipeName { get; }
        IObservable<string> ReceivedString { get; }

        void StartServer();
        bool Write(string text, int connectTimeout = 300);
    }
}
