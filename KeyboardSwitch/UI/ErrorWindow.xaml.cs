using System.Windows;

namespace KeyboardSwitch.UI
{
	public partial class ErrorWindow : Window
	{
		public ErrorWindow()
		{
			this.InitializeComponent();
		}

		public ErrorWindow(Window owner, string text)
			: this()
		{
			this.Owner = owner;

			if (owner != null)
			{
				this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}

			this.textBlock.Text = text;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
