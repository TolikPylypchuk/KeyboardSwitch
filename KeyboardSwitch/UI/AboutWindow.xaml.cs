using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace KeyboardSwitch.UI
{
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			this.InitializeComponent();
		}

		public AboutWindow(Window owner)
			: this()
		{
			this.Owner = owner;
			var version = Assembly.GetExecutingAssembly().GetName().Version;

			this.textBlock.Text = "Keyboard Layout Switch\n" +
				$"Version {version.Major}.{version.Minor}\n" +
				"Created by Tolik Pylypchuk";
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void TextBlock_MouseLeftButtonUp(
			object sender, MouseButtonEventArgs e)
		{
			Process.Start(
				"notepad.exe",
				Path.Combine(
					Path.GetDirectoryName(
						Assembly.GetEntryAssembly().Location) ??
							Environment.CurrentDirectory,
					ConfigurationManager.AppSettings["ReadmeLocation"]));
		}
	}
}
