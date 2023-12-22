namespace KeyboardSwitch.Core.Services.Infrastructure;

public interface INamedPipeService
{
    IObservable<string> ReceivedString { get; }

    void StartServer(string pipeName);

    bool Write(string pipeName, string text, int connectTimeout = 300);
}
