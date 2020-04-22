using System;

namespace KeyboardSwitch.Common.Services.Infrastructure
{
    public interface INamedPipeService
    {
        string NamedPipeName { get; }
        IObservable<string> ReceivedString { get; }

        void StartServer();
        bool Write(string text, int connectTimeout = 300);
    }
}
