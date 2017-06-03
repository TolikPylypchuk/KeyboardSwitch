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
using KeyboardSwitch.Services;

namespace KeyboardSwitch.UI
{
	public partial class SettingsWindow : Window
	{
		private App currentApp = Application.Current as App;
		private bool canSave;
		private bool canClose;
		private bool isBorderClicked;
		private Border focusedBorder;

		public SettingsWindow()
		{
			this.InitializeComponent();

			if (currentApp == null)
			{
				return;
			}
			
			var langs = InputLanguageManager.Current.AvailableInputLanguages
				?.Cast<CultureInfo>()
				.ToList();

			if (langs == null)
			{
				return;
			}
			
			for (int i = 0; i < langs.Count; i++)
			{
				this.langGrid.RowDefinitions.Add(new RowDefinition());
				this.nameGrid.RowDefinitions.Add(new RowDefinition());
			}
			
			int length = LanguageManager.Current.Languages.Values.First().Length;

			for (int i = 0; i < length; i++)
			{
				this.langGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			this.nameGrid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(55)
			});
			
			int langCount = LanguageManager.Current.Languages.Count;
			int row = 0;

			foreach (var lang in langs)
			{
				var langName = new TextBlock
				{
					Text = lang.ToString(),
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
				string languageStr =
					LanguageManager.Current.Languages[lang].ToString();

				foreach (char ch in languageStr)
				{
					this.AddNewBorder(row, col, ch, row + col * langCount);
					col++;
				}

				row++;
			}
		}

		public void BringToForeground()
		{
			App.BringWindowToForeground(this);
			this.scrollViewer.Focus();
		}
		
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
			} else
			{
				this.scrollViewer.ScrollToVerticalOffset(
					this.scrollViewer.VerticalOffset - e.Delta);
			}
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (this.focusedBorder != null &&
				Keyboard.FocusedElement == this.scrollViewer)
			{
				int col = Grid.GetColumn(this.focusedBorder);
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

							foreach (var str in
								LanguageManager.Current.Languages.Values)
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

							foreach (var builder in LanguageManager.Current.Languages.Values)
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

				this.canSave = true;
				this.langGrid.UpdateLayout();

				e.Handled = true;
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (this.focusedBorder != null)
			{
				this.RemoveFocusedBorder();
			}

			this.Hide();

			for (int i = 0; i < LanguageManager.Current.Languages.Count; i++)
			{
				var itemsInRow = this.GetItemsInRow(i);
				int j = 0;

				foreach (var item in itemsInRow)
				{
					var letterBox = (item as Border)?.Child as LetterBox;

					if (letterBox?.Text.Length == 0)
					{
						letterBox.Char = LanguageManager.Current.Languages[
							this.GetLanguage(i)][j];
					}

					j++;
				}
			}

			e.Cancel = !canClose;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			this.tbIcon.Dispose();
			this.currentApp.Shutdown();
		}

		private void StackPanel_MouseLeftButtonDown(
			object sender,
			MouseButtonEventArgs e)
		{
			if (!this.isBorderClicked)
			{
				this.RemoveFocusedBorder();
			}

			this.isBorderClicked = false;
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

		private void LetterBox_GotFocus(object sender, RoutedEventArgs e)
		{
			this.RemoveFocusedBorder();

			var border = (sender as LetterBox)?.Parent as Border;

			if (border == null)
			{
				return;
			}

			this.SetFocusedBorder(border);

			InputLanguageManager.Current.CurrentInputLanguage =
				this.GetLanguage(Grid.GetRow(border));

			e.Handled = true;
		}

		private void LetterBox_LostFocus(object sender, RoutedEventArgs e)
		{
			this.SetBordersBackground(
				this.focusedBorder, Brushes.Transparent);
			this.focusedBorder = null;
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
			this.isBorderClicked = true;
        }

		private void SetFocusedBorder(Border border)
		{
			this.focusedBorder = border;
			this.SetBordersBackground(this.focusedBorder, Brushes.AliceBlue);
		}

		private void RemoveFocusedBorder()
		{
			this.SetBordersBackground(this.focusedBorder, Brushes.Transparent);
			this.focusedBorder = null;
		}

		private void SetBordersBackground(Border border, Brush brush)
		{
			if (border != null)
			{
				int count = LanguageManager.Current.Languages.Keys.Count;
				for (int i = 0; i < count; i++)
				{
					var itemsInRow = this.GetItemsInRow(i);
					foreach (var item in itemsInRow)
					{
						if (Grid.GetColumn(item) == Grid.GetColumn(border))
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
		}

		private void LetterBox_CharChanged(
			object sender,
			CharChangedEventArgs e)
		{
			if (sender is LetterBox letterBox && letterBox.Text.Length > 0)
			{
				canSave = true;

				var border = letterBox.Parent as Border;

				if (border == null)
				{
					return;
				}

				var itemsInRow = GetItemsInRow(Grid.GetRow(border));
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
									letterBox != currentBox &&
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
					LanguageManager.Current.Languages[
						this.GetLanguage(Grid.GetRow(border))][
							Grid.GetColumn(border)] = letterBox.Char;
				}
			}
		}

		private CultureInfo GetLanguage(int index)
		{
			return nameGrid.Children[index] is TextBlock tb
				? new CultureInfo(tb.Text)
				: null;
        }

		private void SetHotKeys_Click(object sender, RoutedEventArgs e)
		{
			new HotKeyWindow(this).ShowDialog();
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			new AboutWindow(this).ShowDialog();
		}
		
		private void ShowWindow_Click(object sender, RoutedEventArgs e)
		{
			this.BringToForeground();
		}

		private void SwitchForward_Click(object sender, RoutedEventArgs e)
		{
			this.currentApp.HotKeyPressed(this.currentApp.HotKeyForward);
		}

		private void SwitchBackward_Click(object sender, RoutedEventArgs e)
		{
			this.currentApp.HotKeyPressed(this.currentApp.HotKeyBackward);
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			this.canClose = true;
			this.Close();
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			for (int i = 0; i < LanguageManager.Current.Languages.Count; i++)
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
							LanguageManager.Current.Languages[GetLanguage(i)][j];
					}

					j++;
				}
			}
			
			if (!FileManager.Current.Write(
				LanguageManager.Current.Languages))
			{
				MessageBox.Show(
					"Couldn't write new info into the file.",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}

			Settings.Default.Save();
			this.canSave = false;

			this.RemoveFocusedBorder();
			this.scrollViewer.Focus();
		}

		private void Save_CanExecute(
			object sender,
			CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.canSave;
		}

		private void SwitchForward_Executed(
			object sender,
			ExecutedRoutedEventArgs e)
		{
			this.currentApp.HotKeyPressed(this.currentApp.HotKeyForward);
		}

		private void SwitchBackward_Executed(
			object sender,
			ExecutedRoutedEventArgs e)
		{
			this.currentApp.HotKeyPressed(this.currentApp.HotKeyBackward);
		}

		private void Switch_CanExecute(
			object sender,
			CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = Clipboard.ContainsText();
		}

		private void New_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.langGrid.ColumnDefinitions.Add(new ColumnDefinition());

			for (int i = 0; i < LanguageManager.Current.Languages.Count; i++)
			{
				this.AddNewBorder(
					i,
					this.langGrid.ColumnDefinitions.Count - 1, ' ',
					this.langGrid.Children.Count);

				LanguageManager.Current.Languages[this.GetLanguage(i)].Append(' ');
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
					var border = item as Border;
					if (border == null)
					{
						continue;
					}

					int focusedCol = Grid.GetColumn(this.focusedBorder);
					int currentCol = Grid.GetColumn(border);

                    if (currentCol > focusedCol)
					{
						Grid.SetColumn(border, currentCol - 1);
					} else if (border != this.focusedBorder &&
						currentCol == focusedCol)
					{
						borderToDelete = border;
					}
				}

				this.langGrid.Children.Remove(borderToDelete);

				LanguageManager.Current.Languages[this.GetLanguage(i)]
					.Remove(
						borderToDelete != null
							? Grid.GetColumn(borderToDelete)
							: Grid.GetColumn(this.focusedBorder),
						1);
			}

			this.langGrid.Children.Remove(this.focusedBorder);
			this.focusedBorder = null;

			this.langGrid.ColumnDefinitions.RemoveAt(
				this.langGrid.ColumnDefinitions.Count - 1);
			this.canSave = true;
		}

		private void Delete_CanExecute(
			object sender,
			CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.focusedBorder != null &&
						   this.langGrid.Children.Count > 2;
		}

		private void Show_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.BringToForeground();
		}

		private IEnumerable<UIElement> GetItemsInRow(int row)
			=> langGrid.Children
					   .Cast<UIElement>()
					   .Where(elem => Grid.GetRow(elem) == row);

		private IEnumerable<UIElement> GetItemsInColumn(int column)
			=> langGrid.Children
					   .Cast<UIElement>()
					   .Where(elem => Grid.GetColumn(elem) == column);
	}
}
