namespace KeyboardSwitch.Settings.State;

using System.Runtime.Serialization;

[DataContract]
public class AppState
{
    [DataMember]
    public double WindowWidth { get; set; }

    [DataMember]
    public double WindowHeight { get; set; }

    [DataMember]
    public int WindowX { get; set; }

    [DataMember]
    public int WindowY { get; set; }

    [DataMember]
    public bool IsWindowMaximized { get; set; }

    [DataMember]
    public bool IsInitialized { get; set; }
}
