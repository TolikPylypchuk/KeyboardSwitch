using System.Windows;

namespace KeyboardSwitch
{
	public partial class ErrorWindow : Window
	{
		public ErrorWindow()
		{
			InitializeComponent();
		}

		public ErrorWindow(Window owner, string text)
		{
			InitializeComponent();
			Owner = owner;
			if (owner != null)
			{
				WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}
			textBlock.Text = text;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
