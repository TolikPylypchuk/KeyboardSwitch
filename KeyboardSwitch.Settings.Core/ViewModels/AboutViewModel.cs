using System.Reflection;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public class AboutViewModel : ReactiveObject
    {
        public AboutViewModel()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.AppVersion = version != null ? $"{version.Major}.{version.Minor}" : "0.0";
        }

        public string AppVersion { get; }
    }
}
