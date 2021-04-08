namespace KeyboardSwitch.Core.Services
{
    public sealed class NoOpSimulator : IUserActivitySimulator
    {
        public void SimulateCopy()
        { }

        public void SimulatePaste()
        { }
    }
}
