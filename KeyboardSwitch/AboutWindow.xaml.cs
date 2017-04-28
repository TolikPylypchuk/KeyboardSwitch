using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace KeyboardSwitch
{
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			InitializeComponent();
		}

		public AboutWindow(Window owner)
		{
			InitializeComponent();
			Owner = owner;
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			textBlock.Text = "Keyboard Layout Switch\n" +
				"Version " + version.Major + "." + version.Minor + "\n" +
				"Created by Tolik Pylypchuk";
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Process.Start("notepad.exe", Path.Combine(Path.GetDirectoryName(
				Assembly.GetEntryAssembly().Location), "readme.txt"));
		}
	}
}
