using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyboardSwitch
{
	public partial class SettingsWindow : Window
	{
		private App currentApp = Application.Current as App;
		private bool canSave = false;
		private bool canClose = false;
		private bool isBorderClicked = false;
		private Border focusedBorder = null;

		public SettingsWindow()
		{
			InitializeComponent();

			var enumerator = currentApp.Languages.Values.GetEnumerator();
			enumerator.MoveNext();
			int length = enumerator.Current.Length;

			int rowCount = (InputLanguageManager.Current.AvailableInputLanguages
				as ArrayList).Count;

			for (int i = 0; i < rowCount; i++)
			{
				langGrid.RowDefinitions.Add(new RowDefinition());
				nameGrid.RowDefinitions.Add(new RowDefinition());
			}

			for (int i = 0; i < length; i++)
			{
				langGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			nameGrid.ColumnDefinitions.Add(new ColumnDefinition()
			{
				Width = new GridLength(55)
			});
			
			int langCount = currentApp.Languages.Count;
			int row = 0;

			foreach (var item in InputLanguageManager.Current.AvailableInputLanguages)
			{
				CultureInfo language = item as CultureInfo;

				TextBlock langName = new TextBlock()
				{
					Text = language.ToString(),
					ToolTip = language.EnglishName,
					Margin = new Thickness(10, 0, 0, 0),
					VerticalAlignment = VerticalAlignment.Center
				};

				var formattedText = new FormattedText(
					langName.Text, CultureInfo.CurrentUICulture,
					FlowDirection.LeftToRight,
					new Typeface(langName.FontFamily, langName.FontStyle,
					langName.FontWeight, langName.FontStretch),
					langName.FontSize, Brushes.Black);

				if (formattedText.Width > nameGrid.ColumnDefinitions[0].Width.Value)
				{
					nameGrid.ColumnDefinitions[0].Width =
						new GridLength(formattedText.Width + 15);
                }

				Grid.SetRow(langName, row);
				nameGrid.Children.Add(langName);

				int col = 0;
				string languageStr = currentApp.Languages[language].ToString();

				foreach (char ch in languageStr)
				{
					AddNewBorder(row, col, ch, row + col * langCount);
					col++;
				}
				row++;
			}
		}

		public void BringToForeground()
		{
			App.BringWindowToForeground(this);
			scrollViewer.Focus();
		}
		
		private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers != ModifierKeys.Control)
			{
				scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
				scrollViewer.ScrollToHorizontalOffset(
					scrollViewer.HorizontalOffset - e.Delta);
			} else
			{
				scrollViewer.ScrollToVerticalOffset(
					scrollViewer.VerticalOffset - e.Delta);
			}
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (focusedBorder != null && Keyboard.FocusedElement == scrollViewer)
			{
				int col = Grid.GetColumn(focusedBorder);
				int offset = 28;

				switch (e.Key)
				{
					case Key.Right:
						if (col != langGrid.ColumnDefinitions.Count - 1)
						{
							var currentColumn = GetItemsInColumn(col).ToArray();
							var nextColumn = GetItemsInColumn(col + 1).ToArray();
							
							foreach (var item in currentColumn)
							{
								Grid.SetColumn(item, col + 1);
							}
							foreach (var item in nextColumn)
							{
								Grid.SetColumn(item, col);
							}
							foreach (StringBuilder str in currentApp.Languages.Values)
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
							var currentColumn = GetItemsInColumn(col).ToArray();
							var prevColumn = GetItemsInColumn(col - 1).ToArray();
							
							foreach (var item in currentColumn)
							{
								Grid.SetColumn(item, col - 1);
							}
							foreach (var item in prevColumn)
							{
								Grid.SetColumn(item, col);
							}
							foreach (StringBuilder str in currentApp.Languages.Values)
							{
								char swap = str[col];
								str[col] = str[col - 1];
								str[col - 1] = swap;
							}
						}
						offset = -offset;
						break;
					default:
						return;
				}

				scrollViewer.ScrollToHorizontalOffset(
							scrollViewer.ContentHorizontalOffset + offset);
				canSave = true;
				langGrid.UpdateLayout();
				e.Handled = true;
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (focusedBorder != null)
			{
				RemoveFocusedBorder();
			}

			Hide();

			for (int i = 0; i < currentApp.Languages.Count; i++)
			{
				var itemsInRow = GetItemsInRow(i);
				int j = 0;
				foreach (var item in itemsInRow)
				{
					LetterBox letterBox = (item as Border).Child as LetterBox;
					if (letterBox.Text.Length == 0)
					{
						letterBox.Char = currentApp.Languages[GetLanguage(i)][j];
					}
					j++;
				}
			}

			e.Cancel = !canClose;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			tbIcon.Dispose();
			currentApp.Shutdown();
		}

		private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!isBorderClicked)
			{
				RemoveFocusedBorder();
			}
			isBorderClicked = false;
		}

		private void AddNewBorder(int row, int col, char ch, int tabIndex)
		{
			Border border = new Border();
			border.ContextMenu = new ContextMenu();
			border.ContextMenuOpening += (s, re) =>
			{
				RemoveFocusedBorder();
				SetFocusedBorder(s as Border);
			};
			border.ContextMenu.Items.Add(new MenuItem()
			{
				Header = "_Delete",
				Command = SwitchCommands.Delete
			});

			LetterBox letterBox = new LetterBox(ch)
			{
				TabIndex = tabIndex
			};

			letterBox.GotFocus += LetterBox_GotFocus;
			letterBox.LostFocus += LetterBox_LostFocus;
			letterBox.CharChanged += LetterBox_CharChanged;

			letterBox.ContextMenu = new ContextMenu();
			letterBox.ContextMenu.Items.Add(new MenuItem()
			{
				Header = "Cu_t",
				Command = ApplicationCommands.Cut
			});
			letterBox.ContextMenu.Items.Add(new MenuItem()
			{
				Header = "_Copy",
				Command = ApplicationCommands.Copy
			});
			letterBox.ContextMenu.Items.Add(new MenuItem()
			{
				Header = "_Paste",
				Command = ApplicationCommands.Paste
			});
			letterBox.ContextMenu.Items.Add(new MenuItem()
			{
				Header = "_Delete",
				Command = SwitchCommands.Delete
			});
			
			border.Child = letterBox;

			Grid.SetRow(border, row);
			Grid.SetColumn(border, col);

			border.MouseLeftButtonDown += Border_MouseLeftButtonDown;

			langGrid.Children.Add(border);
		}

		private void LetterBox_GotFocus(object sender, RoutedEventArgs e)
		{
			RemoveFocusedBorder();

			Border border = (sender as LetterBox).Parent as Border;
			SetFocusedBorder(border);

			InputLanguageManager.Current.CurrentInputLanguage =
				GetLanguage(Grid.GetRow(border));

			e.Handled = true;
		}

		private void LetterBox_LostFocus(object sender, RoutedEventArgs e)
		{
			SetBordersBackground(focusedBorder, Brushes.Transparent);
			focusedBorder = null;
			scrollViewer.Focus();
		}

		private void Border_MouseLeftButtonDown(object sender, RoutedEventArgs e)
		{
			Border border = sender as Border;
            RemoveFocusedBorder();
			scrollViewer.Focus();
			SetFocusedBorder(border);
			isBorderClicked = true;
        }

		private void SetFocusedBorder(Border border)
		{
			focusedBorder = border;
			SetBordersBackground(focusedBorder, Brushes.AliceBlue);
		}

		private void RemoveFocusedBorder()
		{
			SetBordersBackground(focusedBorder, Brushes.Transparent);
			focusedBorder = null;
		}

		private void SetBordersBackground(Border border, Brush brush)
		{
			if (border != null)
			{
				int count = currentApp.Languages.Keys.Count;
				for (int i = 0; i < count; i++)
				{
					var itemsInRow = GetItemsInRow(i);
					foreach (var item in itemsInRow)
					{
						if (Grid.GetColumn(item) == Grid.GetColumn(border))
						{
							(item as Border).Background = brush;
							break;
						}
					}
				}
			}
		}

		private void LetterBox_CharChanged(object sender, CharChangedEventArgs e)
		{
			LetterBox letterBox = sender as LetterBox;

			if (letterBox.Text.Length > 0)
			{
				canSave = true;

				Border border = letterBox.Parent as Border;
				var itemsInRow = GetItemsInRow(Grid.GetRow(border));
				bool isUnique = true;

				if (letterBox.Char != ' ' && (char.IsWhiteSpace(letterBox.Char) ||
					char.IsControl(letterBox.Char)))
				{
					letterBox.Char = ' ';
				}

				if (letterBox.Char != ' ')
				{
					foreach (var item in itemsInRow)
					{
						Border currentBorder = item as Border;
						if (currentBorder != null)
						{
							LetterBox currentBox = currentBorder.Child as LetterBox;

							if (letterBox != currentBox &&
								letterBox.Char == currentBox.Char)
							{
								new ErrorWindow(this,
									"This character is already occupied.").ShowDialog();
								isUnique = false;
								letterBox.Text = string.Empty;
								letterBox.SelectionStart = 0;
								letterBox.SelectionLength = 1;
								break;
							}
						}
					}
				}

				if (isUnique)
				{
					currentApp.Languages[GetLanguage(Grid.GetRow(border))][
						Grid.GetColumn(border)] = letterBox.Char;
                }
			}
		}

		private CultureInfo GetLanguage(int index)
		{
			return new CultureInfo((nameGrid.Children[index] as TextBlock).Text);
        }

		private void SetHotKeys_Click(object sender, RoutedEventArgs e)
		{
			new HotKeyWindow(this).ShowDialog();
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			new AboutWindow(this).ShowDialog();
		}

		private void SystemStartup_Changed(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.Save();
			currentApp.SetStartupShortcut(systemStartupMenuItem.IsChecked);
		}

		private void StartMenuIcon_Changed(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.Save();
			currentApp.SetStartMenuShortcut(startMenuMenuItem.IsChecked);
		}

		private void ShowWindow_Click(object sender, RoutedEventArgs e)
		{
			BringToForeground();
		}

		private void SwitchForward_Click(object sender, RoutedEventArgs e)
		{
			currentApp.HotKeyPressed(currentApp.HotKeyForward);
		}

		private void SwitchBackward_Click(object sender, RoutedEventArgs e)
		{
			currentApp.HotKeyPressed(currentApp.HotKeyBackward);
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			canClose = true;
			Close();
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			for (int i = 0; i < currentApp.Languages.Count; i++)
			{
				var itemsInRow = GetItemsInRow(i);
				int j = 0;
				foreach (var item in itemsInRow)
				{
					LetterBox letterBox = (item as Border).Child as LetterBox;
					if (letterBox.Text.Length == 0)
					{
						letterBox.Char = currentApp.Languages[GetLanguage(i)][j];
					}
					j++;
				}
			}

			bool success = false;
			FileManager.Write(currentApp.Languages, out success);
			if (!success)
			{
				new ErrorWindow(this,
					"Couldn't write new info into the file.").ShowDialog();
			}
			Properties.Settings.Default.Save();
			canSave = false;
			RemoveFocusedBorder();
			scrollViewer.Focus();
		}

		private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = canSave;
		}

		private void SwitchForward_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			currentApp.HotKeyPressed(currentApp.HotKeyForward);
		}

		private void SwitchBackward_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			currentApp.HotKeyPressed(currentApp.HotKeyBackward);
		}

		private void Switch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = Clipboard.ContainsText();
		}

		private void New_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			langGrid.ColumnDefinitions.Add(new ColumnDefinition());

			for (int i = 0; i < currentApp.Languages.Count; i++)
			{
				AddNewBorder(i, langGrid.ColumnDefinitions.Count - 1, ' ',
					langGrid.Children.Count);
				currentApp.Languages[GetLanguage(i)].Append(' ');
			}

			scrollViewer.ScrollToRightEnd();
			Keyboard.Focus((langGrid.Children[langGrid.Children.Count - 2]
				as Border).Child);
		}

		private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			for (int i = 0; i < langGrid.RowDefinitions.Count; i++)
			{
				Border borderToDelete = null;
				var itemsInRow = GetItemsInRow(i);
				foreach (var item in itemsInRow)
				{
					Border border = item as Border;
					int focusedCol = Grid.GetColumn(focusedBorder);
					int currentCol = Grid.GetColumn(border);

                    if (currentCol > focusedCol)
					{
						Grid.SetColumn(border, currentCol - 1);
					} else if (border != focusedBorder && currentCol == focusedCol)
					{
						borderToDelete = border;
					}
				}
				langGrid.Children.Remove(borderToDelete);

				if (borderToDelete != null)
				{
					currentApp.Languages[GetLanguage(i)].Remove(
						Grid.GetColumn(borderToDelete), 1);
				} else
				{
					currentApp.Languages[GetLanguage(i)].Remove(
						Grid.GetColumn(focusedBorder), 1);
				}
			}
			langGrid.Children.Remove(focusedBorder);
			focusedBorder = null;

			langGrid.ColumnDefinitions.RemoveAt(langGrid.ColumnDefinitions.Count - 1);
			canSave = true;
		}

		private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = focusedBorder != null &&
				langGrid.Children.Count > 2;
		}

		private void Show_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			BringToForeground();
		}

		private IEnumerable<UIElement> GetItemsInRow(int row)
		{
			return langGrid.Children.Cast<UIElement>().
					Where(elem => Grid.GetRow(elem) == row);
		}

		private IEnumerable<UIElement> GetItemsInColumn(int column)
		{
			return langGrid.Children.Cast<UIElement>().
					Where(elem => Grid.GetColumn(elem) == column);
		}
	}
}
