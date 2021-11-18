namespace KeyboardSwitch.Services;

public sealed class ExitCodeAccessor : IExitCodeSetter
{
    public ExitCode AppExitCode { get; set; } = ExitCode.Success;
}
