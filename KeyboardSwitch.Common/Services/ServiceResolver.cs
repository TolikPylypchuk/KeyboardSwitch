namespace KeyboardSwitch.Common.Services
{
    public delegate TService ServiceResolver<out TService>(string name);
}
