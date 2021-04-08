namespace KeyboardSwitch.Core.Services
{
    public delegate TService ServiceProvider<out TService>(string name);
}
