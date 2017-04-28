using System.Windows;
using System.Windows.Controls;

namespace KeyboardSwitch
{
	class LetterBox : TextBox
	{
		public static readonly DependencyProperty CharProperty;
		public static readonly RoutedEvent CharChangedEvent;

		public char Char
		{
			get { return (char)GetValue(CharProperty); }
			set { SetValue(CharProperty, value); }
        }

		public event CharChangedEventHandler CharChanged
		{
			add { AddHandler(CharChangedEvent, value); }
			remove { RemoveHandler(CharChangedEvent, value); }
		}

		static LetterBox()
		{
			CharProperty = DependencyProperty.Register("Char", typeof(char),
				typeof(LetterBox), new FrameworkPropertyMetadata(OnCharPropertyChanged));

			CharChangedEvent = EventManager.RegisterRoutedEvent("CharChanged",
				RoutingStrategy.Direct, typeof(CharChangedEventHandler),
				typeof(LetterBox));
		}

		public LetterBox() : this('\0') { Text = string.Empty; }

		public LetterBox(char ch)
		{
			MaxLength = 1;
			TextAlignment = TextAlignment.Center;
			IsUndoEnabled = true;
			Char = ch;
			Text = ch.ToString();

			MouseDoubleClick += SelectAddress;
			GotKeyboardFocus += SelectAddress;

			PreviewMouseLeftButtonDown += (sender, e) =>
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
			};

			TextChanged += (sender, e) =>
			{
				LetterBox box = sender as LetterBox;
                if (box.Text.Length == 0)
				{
					Char = '\0';
				} else
				{
					Char = box.Text[0];
				}
			};

			CharChanged += (sender, e) =>
			{
				LetterBox letterBox = sender as LetterBox;
				letterBox.Text = letterBox.Char == '\0' ? string.Empty :
					letterBox.Char.ToString();
			};
        }

		protected virtual void OnCharChanged(CharChangedEventArgs e)
		{
			RaiseEvent(e);
		}

		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				tb.SelectAll();
			}
		}

		private static void OnCharPropertyChanged(DependencyObject sender,
			DependencyPropertyChangedEventArgs e)
		{
			LetterBox letterBox = sender as LetterBox;
			CharChangedEventArgs args = new CharChangedEventArgs(
				(char)e.OldValue, (char)e.NewValue);
            letterBox.OnCharChanged(args);
		}
	}

	delegate void CharChangedEventHandler(object sender, CharChangedEventArgs e);

	class CharChangedEventArgs : RoutedEventArgs
	{
		public char OldChar { get; set; }
		public char NewChar { get; set; }

		public CharChangedEventArgs(char o, char n)
		{
			RoutedEvent = LetterBox.CharChangedEvent;
			OldChar = o;
			NewChar = n;
		}
	}
}
