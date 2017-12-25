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
			
			switch (Settings.Default.KeyModifiers)
			{
				case KeyModifier.Ctrl | KeyModifier.Shift:
					this.comboBoxDefault.SelectedIndex = 1;
					break;
				case KeyModifier.Alt | KeyModifier.Shift:
					this.comboBoxDefault.SelectedIndex = 2;
					break;
			}

			switch (Settings.Default.InstantKeyModifiers)
			{
				case KeyModifier.Ctrl | KeyModifier.Shift:
					this.comboBoxInstant.SelectedIndex = 1;
					break;
				case KeyModifier.Alt | KeyModifier.Shift:
					this.comboBoxInstant.SelectedIndex = 2;
					break;
			}

			this.model.IndexDefault = this.comboBoxDefault.SelectedIndex;
			this.model.IndexInstant = this.comboBoxInstant.SelectedIndex;

			this.letterBoxForward.Char = Settings.Default.HotKeyForward;
			this.letterBoxBackward.Char = Settings.Default.HotKeyBackward;

			this.letterBoxInstantForward.Char = Settings.Default.HotKeyInstantForward;
			this.letterBoxInstantBackward.Char = Settings.Default.HotKeyInstantBackward;
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
				this.letterBoxBackward.Text.Length != 0 &&
				this.letterBoxInstantForward.Text.Length != 0 &&
				this.letterBoxInstantBackward.Text.Length != 0)
			{
				var modifiers = Settings.Default.KeyModifiers;
				bool modifiersChanged = false;

				if (this.comboBoxDefault.SelectedIndex != this.model.IndexDefault)
				{
					modifiersChanged = true;
					switch (this.comboBoxDefault.SelectedIndex)
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
					this.model.KeyForward != this.model.CurrentApp.HotKeyForward.Key)
				{
					this.model.CurrentApp.HotKeyForward.Dispose();
					this.model.CurrentApp.HotKeyForward = new HotKey(
						this.model.KeyForward,
						modifiers,
						this.model.CurrentApp.HotKeyPressed);

					Settings.Default.HotKeyForward = this.letterBoxForward.Char;
				}

				if (modifiersChanged ||
					this.model.KeyBackward != this.model.CurrentApp.HotKeyBackward.Key)
				{
					this.model.CurrentApp.HotKeyBackward.Dispose();
					this.model.CurrentApp.HotKeyBackward = new HotKey(
						this.model.KeyBackward,
						modifiers,
						this.model.CurrentApp.HotKeyPressed);

					Settings.Default.HotKeyBackward = this.letterBoxBackward.Char;
				}

				modifiers = Settings.Default.InstantKeyModifiers;
				modifiersChanged = false;

				if (this.comboBoxInstant.SelectedIndex != this.model.IndexInstant)
				{
					modifiersChanged = true;
					switch (this.comboBoxInstant.SelectedIndex)
					{
						case 0:
							Settings.Default.InstantKeyModifiers = modifiers =
								KeyModifier.Alt | KeyModifier.Ctrl;
							break;
						case 1:
							Settings.Default.InstantKeyModifiers = modifiers =
								KeyModifier.Ctrl | KeyModifier.Shift;
							break;
						case 2:
							Settings.Default.InstantKeyModifiers = modifiers =
								KeyModifier.Alt | KeyModifier.Shift;
							break;
					}
				}

				if (modifiersChanged ||
					this.model.InstantKeyForward !=
					this.model.CurrentApp.HotKeyInstantForward.Key)
				{
					this.model.CurrentApp.HotKeyInstantForward.Dispose();
					this.model.CurrentApp.HotKeyInstantForward = new HotKey(
						this.model.InstantKeyForward,
						modifiers,
						this.model.CurrentApp.HotKeyPressed);

					Settings.Default.HotKeyInstantForward =
						this.letterBoxInstantForward.Char;
				}

				if (modifiersChanged ||
					this.model.InstantKeyBackward !=
					this.model.CurrentApp.HotKeyInstantBackward.Key)
				{
					this.model.CurrentApp.HotKeyInstantBackward.Dispose();
					this.model.CurrentApp.HotKeyInstantBackward = new HotKey(
						this.model.InstantKeyBackward,
						modifiers,
						this.model.CurrentApp.HotKeyPressed);

					Settings.Default.HotKeyInstantBackward =
						this.letterBoxInstantBackward.Char;
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
					if ((this.letterBoxBackward.Text.Length > 0 &&
					    this.letterBoxForward.Char == this.letterBoxBackward.Char) ||
						(this.comboBoxDefault.SelectedIndex ==
						 this.comboBoxInstant.SelectedIndex &&
						 ((this.letterBoxInstantForward.Text.Length > 0 &&
						   this.letterBoxForward.Char == this.letterBoxInstantForward.Char) ||
						  (this.letterBoxInstantBackward.Text.Length > 0 &&
						   this.letterBoxForward.Char == this.letterBoxInstantBackward.Char))))
					{
						MessageBox.Show(
							"This letter is already occupied",
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
					this.letterBoxForward.Char = Settings.Default.HotKeyForward;
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
					if ((this.letterBoxForward.Text.Length > 0 &&
						 this.letterBoxBackward.Char == this.letterBoxForward.Char) ||
						(this.comboBoxDefault.SelectedIndex ==
						 this.comboBoxInstant.SelectedIndex &&
						 ((this.letterBoxInstantForward.Text.Length > 0 &&
						   this.letterBoxBackward.Char == this.letterBoxInstantForward.Char) ||
						  (this.letterBoxInstantBackward.Text.Length > 0 &&
						   this.letterBoxBackward.Char == this.letterBoxInstantBackward.Char))))
					{
						MessageBox.Show(
							"This letter is already occupied",
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

		public void LetterBoxInstantForward_TextChanged(
			object sender,
			TextChangedEventArgs e)
		{
			if (this.letterBoxInstantForward.Text.Length != 0)
			{
				try
				{
					if ((this.letterBoxInstantBackward.Text.Length > 0 &&
						 this.letterBoxInstantForward.Char ==
						 this.letterBoxInstantBackward.Char) ||
						(this.comboBoxDefault.SelectedIndex ==
						 this.comboBoxInstant.SelectedIndex &&
						 ((this.letterBoxForward.Text.Length > 0 &&
						   this.letterBoxInstantForward.Char == this.letterBoxForward.Char) ||
						  (this.letterBoxBackward.Text.Length > 0 &&
						   this.letterBoxInstantForward.Char == this.letterBoxBackward.Char))))
					{
						MessageBox.Show(
							"This letter is already occupied",
							"Error",
							MessageBoxButton.OK,
							MessageBoxImage.Error);

						this.letterBoxInstantForward.Char = this.model.CharInstantForward;
						return;
					}

					this.model.InstantKeyForward =
						App.GetKey(this.letterBoxInstantForward.Char);
				} catch (ArgumentOutOfRangeException exp)
				{
					MessageBox.Show(
						exp.Message,
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					this.letterBoxInstantForward.Char =
						Settings.Default.HotKeyInstantForward;
				}

				this.model.CharInstantForward = this.letterBoxInstantForward.Char;
			}
		}

		public void LetterBoxInstantBackward_TextChanged(
			object sender,
			TextChangedEventArgs e)
		{
			if (this.letterBoxInstantBackward.Text.Length != 0)
			{
				try
				{
					if ((this.letterBoxInstantForward.Text.Length > 0 &&
						 this.letterBoxInstantBackward.Char ==
						 this.letterBoxInstantForward.Char) ||
						(this.comboBoxDefault.SelectedIndex ==
						 this.comboBoxInstant.SelectedIndex &&
						 ((this.letterBoxForward.Text.Length > 0 &&
						   this.letterBoxInstantBackward.Char == this.letterBoxForward.Char) ||
						  (this.letterBoxBackward.Text.Length > 0 &&
						   this.letterBoxInstantBackward.Char == this.letterBoxBackward.Char))))
					{
						MessageBox.Show(
							"This letter is already occupied",
							"Error",
							MessageBoxButton.OK,
							MessageBoxImage.Error);
						this.letterBoxInstantBackward.Char = this.model.CharInstantBackward;
						return;
					}

					this.model.InstantKeyBackward =
						App.GetKey(this.letterBoxInstantBackward.Char);
				} catch (ArgumentOutOfRangeException exp)
				{
					MessageBox.Show(
						exp.Message,
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					this.letterBoxInstantBackward.Char =
						Settings.Default.HotKeyInstantBackward;
				}

				this.model.CharInstantBackward = this.letterBoxInstantBackward.Char;
			}
		}

		#endregion
	}
}
