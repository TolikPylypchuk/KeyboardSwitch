using System.Windows;

namespace KeyboardSwitch
{
	public class CharChangedEventArgs : RoutedEventArgs
	{
		public char OldChar { get; set; }
		public char NewChar { get; set; }

		public CharChangedEventArgs(char oldChar, char newChar)
		{
			this.RoutedEvent = LetterBox.CharChangedEvent;
			this.OldChar = oldChar;
			this.NewChar = newChar;
		}
	}
}
