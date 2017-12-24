using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using KeyboardSwitch.Properties;

namespace KeyboardSwitch.UI
{
	public partial class SettingsWindow : Window
	{
		#region Constructors

		public SettingsWindow(SettingsViewModel model)
		{
			this.InitializeComponent();

			this.Model = model;

			this.InitializeLanguages();
		}

		#endregion

		#region Properties

		public SettingsViewModel Model { get; }

		#endregion

		#region Methods

		public void BringToForeground()
		{
			App.BringWindowToForeground(this);
			this.scrollViewer.Focus();
		}

		private void InitializeLanguages()
		{
			var langs = this.Model.CurrentApp.LanguageManager
				.InputLanguageManager.InputLanguages.ToList();

			for (int i = 0; i < langs.Count; i++)
			{
				this.langGrid.RowDefinitions.Add(new RowDefinition());
				this.nameGrid.RowDefinitions.Add(new RowDefinition());
			}

			int length = this.Model.CurrentApp.LanguageManager
				.Languages.Values.First().Length;

			for (int i = 0; i < length; i++)
			{
				this.langGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			this.nameGrid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(55)
			});

			int langCount = this.Model.CurrentApp.LanguageManager
				.Languages.Count;
			int row = 0;

			foreach (var lang in langs)
			{
				var langName = new TextBlock
				{
					Text = CultureInfo.InvariantCulture.TextInfo
						.ToTitleCase(lang.ThreeLetterISOLanguageName),
					ToolTip = lang.EnglishName,
					Margin = new Thickness(10, 0, 0, 0),
					VerticalAlignment = VerticalAlignment.Center
				};

				var formattedText = new FormattedText(
					langName.Text,
					CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight,
					new Typeface(
						langName.FontFamily,
						langName.FontStyle,
						langName.FontWeight,
						langName.FontStretch),
					langName.FontSize,
					Brushes.Black,
					VisualTreeHelper.GetDpi(this).PixelsPerDip);

				if (formattedText.Width >
					this.nameGrid.ColumnDefinitions[0].Width.Value)
				{
					this.nameGrid.ColumnDefinitions[0].Width =
						new GridLength(formattedText.Width + 15);
				}

				Grid.SetRow(langName, row);
				this.nameGrid.Children.Add(langName);

				int col = 0;
				string languageStr = this.Model.CurrentApp.LanguageManager
					.Languages[lang].ToString();

				foreach (char ch in languageStr)
				{
					this.AddNewBorder(row, col, ch, row + col * langCount);
					col++;
				}

				row++;
			}
		}
		
		private void AddNewBorder(int row, int col, char ch, int tabIndex)
		{
			var border = new Border { ContextMenu = new ContextMenu() };

			border.ContextMenuOpening += (s, re) =>
			{
				this.RemoveFocusedBorder();
				this.SetFocusedBorder(s as Border);
			};

			border.ContextMenu?.Items.Add(new MenuItem
			{
				Header = "_Delete",
				Command = SwitchCommands.Delete
			});

			var letterBox = new LetterBox(ch)
			{
				TabIndex = tabIndex
			};

			letterBox.GotFocus += this.LetterBox_GotFocus;
			letterBox.LostFocus += this.LetterBox_LostFocus;
			letterBox.CharChanged += this.LetterBox_CharChanged;

			letterBox.ContextMenu = new ContextMenu();
			letterBox.ContextMenu.Items.Add(new MenuItem
			{
				Header = "Cu_t",
				Command = ApplicationCommands.Cut
			});

			letterBox.ContextMenu.Items.Add(new MenuItem
			{
				Header = "_Copy",
				Command = ApplicationCommands.Copy
			});

			letterBox.ContextMenu.Items.Add(new MenuItem
			{
				Header = "_Paste",
				Command = ApplicationCommands.Paste
			});

			letterBox.ContextMenu.Items.Add(new MenuItem
			{
				Header = "_Delete",
				Command = SwitchCommands.Delete
			});

			border.Child = letterBox;

			Grid.SetRow(border, row);
			Grid.SetColumn(border, col);

			border.MouseLeftButtonDown += this.Border_MouseLeftButtonDown;

			this.langGrid.Children.Add(border);
		}
		
		private void SetFocusedBorder(Border border)
		{
			this.Model.FocusedBorder = border;
			this.SetBackground(this.Model.FocusedBorder, Brushes.AliceBlue);
		}

		private void RemoveFocusedBorder()
		{
			this.SetBackground(this.Model.FocusedBorder, Brushes.Transparent);
			this.Model.FocusedBorder = null;
		}

		private void SetBackground(UIElement element, Brush brush)
		{
			if (element == null)
			{
				return;
			}

			int count = this.Model.CurrentApp.LanguageManager
				.Languages.Keys.Count;

			for (int i = 0; i < count; i++)
			{
				var itemsInRow = this.GetItemsInRow(i);
				foreach (var item in itemsInRow)
				{
					if (Grid.GetColumn(item) == Grid.GetColumn(element))
					{
						if (item is Border b)
						{
							b.Background = brush;
						}

						break;
					}
				}
			}
		}

		private CultureInfo GetLanguage(int index)
			=> this.nameGrid.Children[index] is TextBlock tb
				? new CultureInfo(tb.Text)
				: null;

		private IEnumerable<UIElement> GetItemsInRow(int row)
			=> this.langGrid.Children
				.Cast<UIElement>()
				.Where(elem => Grid.GetRow(elem) == row);

		private IEnumerable<UIElement> GetItemsInColumn(int column)
			=> this.langGrid.Children
				.Cast<UIElement>()
				.Where(elem => Grid.GetColumn(elem) == column);

		#endregion

		#region Event handlers

		private void Window_PreviewMouseWheel(
			object sender,
			MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers != ModifierKeys.Control)
			{
				this.scrollViewer.ScrollToVerticalOffset(
					this.scrollViewer.VerticalOffset);

				this.scrollViewer.ScrollToHorizontalOffset(
					this.scrollViewer.HorizontalOffset - e.Delta);
			}
			else
			{
				this.scrollViewer.ScrollToVerticalOffset(
					this.scrollViewer.VerticalOffset - e.Delta);
			}
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (this.Model.FocusedBorder == null ||
				!Keyboard.FocusedElement.Equals(this.scrollViewer))
			{
				return;
			}

			int col = Grid.GetColumn(this.Model.FocusedBorder);
			int offset = 28;

			switch (e.Key)
			{
				case Key.Right:
					if (col != this.langGrid.ColumnDefinitions.Count - 1)
					{
						var currentColumn =
							this.GetItemsInColumn(col).ToArray();

						var nextColumn =
							this.GetItemsInColumn(col + 1).ToArray();

						foreach (var item in currentColumn)
						{
							Grid.SetColumn(item, col + 1);
						}

						foreach (var item in nextColumn)
						{
							Grid.SetColumn(item, col);
						}

						foreach (var str in this.Model.CurrentApp
							.LanguageManager.Languages.Values)
						{
							char swap = str[col];
							str[col] = str[col + 1];
							str[col + 1] = swap;
						}
					}
					break;
				case Key.Left:
					if (col != 0)
					{
						var currentColumn =
							this.GetItemsInColumn(col).ToArray();
						var prevColumn =
							this.GetItemsInColumn(col - 1).ToArray();

						foreach (var item in currentColumn)
						{
							Grid.SetColumn(item, col - 1);
						}

						foreach (var item in prevColumn)
						{
							Grid.SetColumn(item, col);
						}

						foreach (var builder in this.Model.CurrentApp
							.LanguageManager.Languages.Values)
						{
							char swap = builder[col];
							builder[col] = builder[col - 1];
							builder[col - 1] = swap;
						}
					}

					offset = -offset;
					break;
				default:
					return;
			}

			this.scrollViewer.ScrollToHorizontalOffset(
				this.scrollViewer.ContentHorizontalOffset + offset);

			this.Model.CanSave = true;
			this.langGrid.UpdateLayout();

			e.Handled = true;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (this.Model.FocusedBorder != null)
			{
				this.RemoveFocusedBorder();
			}

			this.Hide();

			for (int i = 0;
				i < this.Model.CurrentApp.LanguageManager.Languages.Count;
				i++)
			{
				var itemsInRow = this.GetItemsInRow(i);
				int j = 0;

				foreach (var item in itemsInRow)
				{
					var letterBox = (item as Border)?.Child as LetterBox;

					if (letterBox?.Text.Length == 0)
					{
						letterBox.Char =
							this.Model.CurrentApp.LanguageManager.Languages[
								this.GetLanguage(i)][j];
					}

					j++;
				}
			}

			e.Cancel = !this.Model.CanClose;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			this.tbIcon.Dispose();
			this.Model.CurrentApp.Shutdown();
		}

		private void StackPanel_MouseLeftButtonDown(
			object sender,
			MouseButtonEventArgs e)
		{
			if (!this.Model.IsBorderClicked)
			{
				this.RemoveFocusedBorder();
			}

			this.Model.IsBorderClicked = false;
		}

		private void LetterBox_GotFocus(object sender, RoutedEventArgs e)
		{
			this.RemoveFocusedBorder();

			if ((sender as LetterBox)?.Parent is Border border)
			{
				this.SetFocusedBorder(border);

				InputLanguageManager.Current.CurrentInputLanguage =
					this.GetLanguage(Grid.GetRow(border));

				e.Handled = true;
			}
		}

		private void LetterBox_LostFocus(object sender, RoutedEventArgs e)
		{
			this.SetBackground(
				this.Model.FocusedBorder, Brushes.Transparent);
			this.Model.FocusedBorder = null;
			this.scrollViewer.Focus();
		}

		private void Border_MouseLeftButtonDown(
			object sender,
			RoutedEventArgs e)
		{
			var border = sender as Border;
			this.RemoveFocusedBorder();
			this.scrollViewer.Focus();
			this.SetFocusedBorder(border);
			this.Model.IsBorderClicked = true;
		}

		private void LetterBox_CharChanged(
			object sender,
			CharChangedEventArgs e)
		{
			if (!(sender is LetterBox letterBox) || letterBox.Text.Length <= 0)
			{
				return;
			}

			this.Model.CanSave = true;

			if (letterBox.Parent is Border border)
			{
				var itemsInRow = this.GetItemsInRow(Grid.GetRow(border));
				bool isUnique = true;

				if (letterBox.Char != ' ' &&
					(Char.IsWhiteSpace(letterBox.Char) ||
					 Char.IsControl(letterBox.Char)))
				{
					letterBox.Char = ' ';
				}

				if (letterBox.Char != ' ')
				{
					if (itemsInRow.OfType<Border>()
						.Select(currentBorder =>
							currentBorder.Child as LetterBox)
						.Any(currentBox =>
							!letterBox.Equals(currentBox) &&
							letterBox.Char == currentBox?.Char))
					{
						MessageBox.Show(
							"This character is already occupied.",
							"Error",
							MessageBoxButton.OK,
							MessageBoxImage.Error);

						isUnique = false;
						letterBox.Text = string.Empty;
						letterBox.SelectionStart = 0;
						letterBox.SelectionLength = 1;
					}
				}

				if (isUnique)
				{
					this.Model.CurrentApp.LanguageManager.Languages[
						this.GetLanguage(Grid.GetRow(border))][
						Grid.GetColumn(border)] = letterBox.Char;
				}
			}
		}

		private void SetHotKeys_Click(object sender, RoutedEventArgs e)
			=> new HotKeyWindow { Owner = this }.ShowDialog();

		private void About_Click(object sender, RoutedEventArgs e)
			=> new AboutWindow { Owner = this }.ShowDialog();

		private void ShowWindow_Click(object sender, RoutedEventArgs e)
			=> this.BringToForeground();

		private void SwitchForward_Click(object sender, RoutedEventArgs e)
			=> this.Model.CurrentApp.SwitchText(
				true, this.Model.DefaultTextManager);

		private void SwitchBackward_Click(object sender, RoutedEventArgs e)
			=> this.Model.CurrentApp.SwitchText(
				false, this.Model.DefaultTextManager);

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			this.Model.CanClose = true;
			this.Close();
		}

		#endregion

		#region Command bindings

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			for (int i = 0;
					i < this.Model.CurrentApp.LanguageManager.Languages.Count;
					i++)
			{
				var itemsInRow = this.GetItemsInRow(i);
				int j = 0;
				foreach (var item in itemsInRow)
				{
					if (item is Border b &&
						b.Child is LetterBox letterBox &&
						letterBox.Text.Length == 0)
					{
						letterBox.Char =
							this.Model.CurrentApp.LanguageManager.Languages[
								this.GetLanguage(i)][j];
					}

					j++;
				}
			}
			
			if (!this.Model.CurrentApp.FileManager.Write(
				this.Model.LanguageManager.Languages))
			{
				MessageBox.Show(
					"Couldn't write new info into the file.",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}

			Settings.Default.Save();
			this.Model.CanSave = false;

			this.RemoveFocusedBorder();
			this.scrollViewer.Focus();
		}

		private void Save_CanExecute(
			object sender,
			CanExecuteRoutedEventArgs e)
			=> e.CanExecute = this.Model.CanSave;

		private void SwitchForward_Executed(
			object sender,
			ExecutedRoutedEventArgs e)
			=> this.Model.CurrentApp.SwitchText(
				true, this.Model.DefaultTextManager);

		private void SwitchBackward_Executed(
			object sender,
			ExecutedRoutedEventArgs e)
			=> this.Model.CurrentApp.SwitchText(
				false, this.Model.DefaultTextManager);

		private void Switch_CanExecute(
			object sender,
			CanExecuteRoutedEventArgs e)
			=> e.CanExecute = this.Model.DefaultTextManager.HasText;

		private void New_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.langGrid.ColumnDefinitions.Add(new ColumnDefinition());

			for (int i = 0;
					i < this.Model.CurrentApp.LanguageManager.Languages.Count;
					i++)
			{
				this.AddNewBorder(
					i,
					this.langGrid.ColumnDefinitions.Count - 1, ' ',
					this.langGrid.Children.Count);

				this.Model.CurrentApp.LanguageManager.Languages[
					this.GetLanguage(i)].Append(' ');
			}

			this.scrollViewer.ScrollToRightEnd();

			if (this.langGrid.Children[
					this.langGrid.Children.Count - 2] is Border b)
			{
				Keyboard.Focus(b.Child);
			}
		}

		private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			for (int i = 0; i < this.langGrid.RowDefinitions.Count; i++)
			{
				Border borderToDelete = null;
				var itemsInRow = this.GetItemsInRow(i);

				foreach (var item in itemsInRow)
				{
					if (item is Border border)
					{
						int focusedCol = Grid.GetColumn(this.Model.FocusedBorder);
						int currentCol = Grid.GetColumn(border);

						if (currentCol > focusedCol)
						{
							Grid.SetColumn(border, currentCol - 1);
						} else if (!border.Equals(this.Model.FocusedBorder) &&
								   currentCol == focusedCol)
						{
							borderToDelete = border;
						}
					}
				}

				this.langGrid.Children.Remove(borderToDelete);

				this.Model.CurrentApp.LanguageManager.Languages[
					this.GetLanguage(i)].Remove(
						borderToDelete != null
							? Grid.GetColumn(borderToDelete)
							: Grid.GetColumn(this.Model.FocusedBorder),
						1);
			}

			this.langGrid.Children.Remove(this.Model.FocusedBorder);
			this.Model.FocusedBorder = null;

			this.langGrid.ColumnDefinitions.RemoveAt(
				this.langGrid.ColumnDefinitions.Count - 1);
			this.Model.CanSave = true;
		}

		private void Delete_CanExecute(
			object sender,
			CanExecuteRoutedEventArgs e)
			=> e.CanExecute = this.Model.FocusedBorder != null &&
				this.langGrid.Children.Count > 2;

		private void Show_Executed(object sender, ExecutedRoutedEventArgs e)
			=> this.BringToForeground();

		#endregion
	}
}
