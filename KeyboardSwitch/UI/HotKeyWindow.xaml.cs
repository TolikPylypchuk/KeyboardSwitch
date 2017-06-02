using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using KeyboardSwitch.Properties;
using KeyboardSwitch.Services;

namespace KeyboardSwitch.UI
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
			this.InitializeComponent();
		}

		public HotKeyWindow(Window owner)
			: this()
		{
			this.Owner = owner;

			this.letterBoxForward.Char = Settings.Default.HotKeyForward;
			this.letterBoxBackward.Char = Settings.Default.HotKeyBackward;

			this.charF = Settings.Default.HotKeyForward;
			this.charB = Settings.Default.HotKeyBackward;

			this.keyF = App.GetKey(charF);
			this.keyB = App.GetKey(charB);

			switch (Settings.Default.KeyModifiers)
			{
				case KeyModifier.Ctrl | KeyModifier.Shift:
					this.checkBox.SelectedIndex = 1;
					break;
				case KeyModifier.Alt | KeyModifier.Shift:
					this.checkBox.SelectedIndex = 2;
					break;
			}

			this.index = checkBox.SelectedIndex;
		}

		private void BtnOK_Click(object sender, RoutedEventArgs e)
		{
			if (this.letterBoxBackward.Text.Length != 0 &&
				this.letterBoxBackward.Text.Length != 0)
			{
				var modifiers = Settings.Default.KeyModifiers;
				bool modifiersChanged = false;

				if (this.checkBox.SelectedIndex != index)
				{
					modifiersChanged = true;
					switch (this.checkBox.SelectedIndex)
					{
						case 0:
							Settings.Default.KeyModifiers = modifiers =
								KeyModifier.Alt | KeyModifier.Ctrl;
							break;
						case 1:
							Settings.Default.KeyModifiers = modifiers =
								KeyModifier.Ctrl | KeyModifier.Shift;
							break;
						case 2:
							Settings.Default.KeyModifiers = modifiers =
								KeyModifier.Alt | KeyModifier.Shift;
							break;
					}
				}

				if (modifiersChanged ||
					keyF != this.currentApp.HotKeyForward.Key)
				{
					this.currentApp.HotKeyForward.Dispose();
					this.currentApp.HotKeyForward = new HotKey(
						keyF, modifiers, this.currentApp.HotKeyPressed);

					Settings.Default.HotKeyForward =
						this.letterBoxForward.Char;
				}

				if (modifiersChanged ||
					keyB != this.currentApp.HotKeyBackward.Key)
				{
					this.currentApp.HotKeyBackward.Dispose();
					this.currentApp.HotKeyBackward = new HotKey(
						keyB, modifiers, this.currentApp.HotKeyPressed);

					Settings.Default.HotKeyBackward =
						this.letterBoxBackward.Char;
				}

				Settings.Default.Save();
			}

			this.Close();
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		
		public void LetterBoxForward_TextChanged(
			object sender,
			TextChangedEventArgs e)
		{
			if (this.letterBoxForward.Text.Length != 0)
			{
				try
				{
					if (this.letterBoxBackward.Text.Length > 0 &&
					    this.letterBoxForward.Char == letterBoxBackward.Char)
					{
						new ErrorWindow(
							this,
							"This letter is already set as the\n" + 
							"backward hot key.")
							.ShowDialog();

						this.letterBoxForward.Char = this.charF;
						return;
					}

					this.keyF = App.GetKey(this.letterBoxForward.Char);
				} catch (ArgumentOutOfRangeException exp)
				{
					new ErrorWindow(this, exp.ParamName).ShowDialog();
					this.letterBoxForward.Char =
						Settings.Default.HotKeyForward;
				}

				this.charF = this.letterBoxForward.Char;
			}
		}

		public void LetterBoxBackward_TextChanged(
			object sender,
			TextChangedEventArgs e)
		{
			if (this.letterBoxBackward.Text.Length != 0)
			{
				try
				{
					if (this.letterBoxForward.Text.Length > 0 &&
					    this.letterBoxBackward.Char == letterBoxForward.Char)
					{
						new ErrorWindow(
							this,
							"This letter is already set as the\n" +
							"forward hot key.")
							.ShowDialog();
						this.letterBoxBackward.Char = this.charB;
						return;
					}

					this.keyB = App.GetKey(this.letterBoxBackward.Char);
				} catch (ArgumentOutOfRangeException exp)
				{
					new ErrorWindow(this, exp.ParamName).ShowDialog();
					this.letterBoxBackward.Char =
						Settings.Default.HotKeyBackward;
				}

				this.charB = this.letterBoxBackward.Char;
            }
		}

		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			(sender as TextBox)?.SelectAll();
		}

		private void IgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			if (sender is TextBox tb)
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
