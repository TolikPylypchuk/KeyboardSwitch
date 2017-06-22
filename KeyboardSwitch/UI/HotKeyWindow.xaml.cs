using System;
using System.Windows;
using System.Windows.Controls;

using KeyboardSwitch.Interop;
using KeyboardSwitch.Properties;
using KeyboardSwitch.Services;

namespace KeyboardSwitch.UI
{
	public partial class HotKeyWindow : Window
	{
		#region Constructors

		public HotKeyWindow()
		{
			this.InitializeComponent();

			this.letterBoxForward.Char = Settings.Default.HotKeyForward;
			this.letterBoxBackward.Char = Settings.Default.HotKeyBackward;

			switch (Settings.Default.KeyModifiers)
			{
				case KeyModifier.Ctrl | KeyModifier.Shift:
					this.comboBox.SelectedIndex = 1;
					break;
				case KeyModifier.Alt | KeyModifier.Shift:
					this.comboBox.SelectedIndex = 2;
					break;
			}

			this.model.Index = this.comboBox.SelectedIndex;
		}

		#endregion

		#region Methods
		
		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			(sender as TextBox)?.SelectAll();
		}

		#endregion

		#region Event handlers

		private void BtnOK_Click(object sender, RoutedEventArgs e)
		{
			if (this.letterBoxBackward.Text.Length != 0 &&
				this.letterBoxBackward.Text.Length != 0)
			{
				var modifiers = Settings.Default.KeyModifiers;
				bool modifiersChanged = false;

				if (this.comboBox.SelectedIndex != this.model.Index)
				{
					modifiersChanged = true;
					switch (this.comboBox.SelectedIndex)
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
					this.model.KeyForward !=
					this.model.CurrentApp.HotKeyForward.Key)
				{
					this.model.CurrentApp.HotKeyForward.Dispose();
					this.model.CurrentApp.HotKeyForward = new HotKey(
						this.model.KeyForward,
						modifiers,
						this.model.CurrentApp.HotKeyPressed);

					Settings.Default.HotKeyForward =
						this.letterBoxForward.Char;
				}

				if (modifiersChanged ||
					this.model.KeyBackward !=
					this.model.CurrentApp.HotKeyBackward.Key)
				{
					this.model.CurrentApp.HotKeyBackward.Dispose();
					this.model.CurrentApp.HotKeyBackward = new HotKey(
						this.model.KeyBackward,
						modifiers,
						this.model.CurrentApp.HotKeyPressed);

					Settings.Default.HotKeyBackward =
						this.letterBoxBackward.Char;
				}

				Settings.Default.Save();
			}

			this.Close();
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
			=> this.Close();
		
		public void LetterBoxForward_TextChanged(
			object sender,
			TextChangedEventArgs e)
		{
			if (this.letterBoxForward.Text.Length != 0)
			{
				try
				{
					if (this.letterBoxBackward.Text.Length > 0 &&
					    this.letterBoxForward.Char ==
						this.letterBoxBackward.Char)
					{
						MessageBox.Show(
							"This letter is already set as the\n" + 
							"backward hot key.",
							"Error",
							MessageBoxButton.OK,
							MessageBoxImage.Error);

						this.letterBoxForward.Char = this.model.CharForward;
						return;
					}

					this.model.KeyForward = App.GetKey(
						this.letterBoxForward.Char);
				} catch (ArgumentOutOfRangeException exp)
				{
					MessageBox.Show(
						exp.Message,
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					this.letterBoxForward.Char =
						Settings.Default.HotKeyForward;
				}

				this.model.CharForward = this.letterBoxForward.Char;
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
					    this.letterBoxBackward.Char ==
						this.letterBoxForward.Char)
					{
						MessageBox.Show(
							"This letter is already set as the\n" +
							"forward hot key.",
							"Error",
							MessageBoxButton.OK,
							MessageBoxImage.Error);
						this.letterBoxBackward.Char = this.model.CharBackward;
						return;
					}

					this.model.KeyBackward =
						App.GetKey(this.letterBoxBackward.Char);
				} catch (ArgumentOutOfRangeException exp)
				{
					MessageBox.Show(
						exp.Message,
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					this.letterBoxBackward.Char =
						Settings.Default.HotKeyBackward;
				}

				this.model.CharBackward = this.letterBoxBackward.Char;
            }
		}

		#endregion
	}
}
