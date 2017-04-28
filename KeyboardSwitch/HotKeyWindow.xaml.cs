using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeyboardSwitch
{
	public partial class HotKeyWindow : Window
	{
		private App currentApp = Application.Current as App;

		private Key keyF;
		private Key keyB;

		private char charF;
		private char charB;
		private int index;

        public HotKeyWindow()
		{
			InitializeComponent();
		}

		public HotKeyWindow(Window owner)
		{
			InitializeComponent();
			Owner = owner;
			letterBoxForward.Char = Properties.Settings.Default.HotKeyForward;
			letterBoxBackward.Char = Properties.Settings.Default.HotKeyBackward;

			charF = Properties.Settings.Default.HotKeyForward;
			charB = Properties.Settings.Default.HotKeyBackward;

			keyF = App.GetKey(charF);
			keyB = App.GetKey(charB);

			switch (Properties.Settings.Default.KeyModifiers)
			{
				case "CS":
					checkBox.SelectedIndex = 1;
					break;
				case "AS":
					checkBox.SelectedIndex = 2;
					break;
			}

			index = checkBox.SelectedIndex;
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			if (letterBoxBackward.Text.Length != 0 && letterBoxBackward.Text.Length != 0)
			{
				KeyModifier modifiers = App.GetKeyModifiers();
				bool modifiersChanged = false;

				if (checkBox.SelectedIndex != index)
				{
					modifiersChanged = true;
					switch (checkBox.SelectedIndex)
					{
						case 0:
							modifiers = KeyModifier.Ctrl | KeyModifier.Alt;
							Properties.Settings.Default.KeyModifiers = "CA";
							break;
						case 1:
							modifiers = KeyModifier.Ctrl | KeyModifier.Shift;
							Properties.Settings.Default.KeyModifiers = "CS";
							break;
						case 2:
							modifiers = KeyModifier.Alt | KeyModifier.Shift;
							Properties.Settings.Default.KeyModifiers = "AS";
							break;
					}
				}

				if (modifiersChanged || keyF != currentApp.HotKeyForward.Key)
				{
					currentApp.HotKeyForward.Dispose();
					currentApp.HotKeyForward = new HotKey(keyF, modifiers,
						currentApp.HotKeyPressed);

					Properties.Settings.Default.HotKeyForward = letterBoxForward.Char;
				}

				if (modifiersChanged || keyB != currentApp.HotKeyBackward.Key)
				{
					currentApp.HotKeyBackward.Dispose();
					currentApp.HotKeyBackward = new HotKey(keyB, modifiers,
						currentApp.HotKeyPressed);

					Properties.Settings.Default.HotKeyBackward = letterBoxBackward.Char;
				}

				Properties.Settings.Default.Save();
			}

			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		
		public void letterBoxForward_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (letterBoxForward.Text.Length != 0)
			{
				try
				{
					if (letterBoxBackward.Text.Length > 0 &&
						letterBoxForward.Char == letterBoxBackward.Char)
					{
						new ErrorWindow(this, "This letter is already set as the\n" + 
							"backward hot key.").ShowDialog();
						letterBoxForward.Char = charF;
						return;
					}
					keyF = App.GetKey(letterBoxForward.Char);
				} catch (ArgumentOutOfRangeException exp)
				{
					new ErrorWindow(this, exp.ParamName).ShowDialog();
					letterBoxForward.Char = Properties.Settings.Default.HotKeyForward;
				}

				charF = letterBoxForward.Char;
			}
		}

		public void letterBoxBackward_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (letterBoxBackward.Text.Length != 0)
			{
				try
				{
					if (letterBoxForward.Text.Length > 0 &&
						letterBoxBackward.Char == letterBoxForward.Char)
					{
						new ErrorWindow(this, "This letter is already set as the\n" +
							"forward hot key.").ShowDialog();
						letterBoxBackward.Char = charB;
						return;
					}
					keyB = App.GetKey(letterBoxBackward.Char);
				} catch (ArgumentOutOfRangeException exp)
				{
					new ErrorWindow(this, exp.ParamName).ShowDialog();
					letterBoxBackward.Char = Properties.Settings.Default.HotKeyBackward;
				}

				charB = letterBoxBackward.Char;
            }
		}

		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				tb.SelectAll();
			}
		}

		private void IgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				if (!tb.IsKeyboardFocusWithin)
				{
					e.Handled = true;
					tb.Focus();
				}
			}
		}
	}
}
