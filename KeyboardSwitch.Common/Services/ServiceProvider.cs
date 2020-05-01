namespace KeyboardSwitch.Common.Services
{
    public delegate TService ServiceProvider<out TService>(string name);
}
