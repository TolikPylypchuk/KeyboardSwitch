using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KeyboardSwitch.Settings.Views
{
    public class SandboxView : UserControl
    {
        public SandboxView()
            => this.InitializeComponent();

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}
