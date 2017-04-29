using System;
using System.Windows;
using System.Windows.Controls;

namespace KeyboardSwitch.UI
{
	public class LetterBox : TextBox
	{
		public static readonly DependencyProperty CharProperty;
		public static readonly RoutedEvent CharChangedEvent;

		public char Char
		{
			get => (char)this.GetValue(CharProperty);
			set => this.SetValue(CharProperty, value);
		}

		public event CharChangedEventHandler CharChanged
		{
			add => this.AddHandler(CharChangedEvent, value);
			remove => this.RemoveHandler(CharChangedEvent, value);
		}

		static LetterBox()
		{
			CharProperty = DependencyProperty.Register(
				nameof(Char),
				typeof(char),
				typeof(LetterBox),
				new FrameworkPropertyMetadata(OnCharPropertyChanged));

			CharChangedEvent = EventManager.RegisterRoutedEvent(
				nameof(CharChanged),
				RoutingStrategy.Direct,
				typeof(CharChangedEventHandler),
				typeof(LetterBox));
		}

		public LetterBox()
			: this('\0')
		{
			this.Text = string.Empty;
		}

		public LetterBox(char ch)
		{
			this.MaxLength = 1;
			this.TextAlignment = TextAlignment.Center;
			this.IsUndoEnabled = true;
			this.Char = ch;
			this.Text = ch.ToString();

			this.MouseDoubleClick += this.SelectAddress;
			this.GotKeyboardFocus += this.SelectAddress;

			this.PreviewMouseLeftButtonDown += (sender, e) =>
			{
				if (sender is TextBox tb)
				{
					if (!tb.IsKeyboardFocusWithin)
					{
						e.Handled = true;
						tb.Focus();
					}
				}
			};

			this.TextChanged += (sender, e) =>
			{
				if (sender is LetterBox box)
				{
					this.Char = box.Text.Length == 0
						? '\0'
						: box.Text[0];
				}
			};

			this.CharChanged += (sender, e) =>
			{
				if (sender is LetterBox box)
				{
					box.Text = box.Char == '\0'
						? String.Empty
						: box.Char.ToString();
				}
			};
        }

		protected virtual void OnCharChanged(CharChangedEventArgs e)
		{
			this.RaiseEvent(e);
		}

		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			(sender as TextBox)?.SelectAll();
		}

		private static void OnCharPropertyChanged(
			DependencyObject sender,
			DependencyPropertyChangedEventArgs e)
		{
			var letterBox = sender as LetterBox;
			var args = new CharChangedEventArgs(
				(char)e.OldValue, (char)e.NewValue);

            letterBox?.OnCharChanged(args);
		}
	}
}
