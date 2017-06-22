using System.Windows.Input;

namespace KeyboardSwitch.UI
{
	public static class SwitchCommands
	{
		public static RoutedUICommand Forward { get; }
		public static RoutedUICommand Backward { get; }
		public static RoutedUICommand Delete { get; }
		public static RoutedUICommand Show { get; }

		static SwitchCommands()
		{
			var input = new InputGestureCollection
			{
				new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F")
			};

			Forward = new RoutedUICommand(
				"Forward",
				nameof(Forward),
				typeof(SwitchCommands),
				input);

			input = new InputGestureCollection
			{
				new KeyGesture(Key.B, ModifierKeys.Control, "Ctrl+B")
			};

			Backward = new RoutedUICommand(
				"Backward",
				nameof(Backward),
				typeof(SwitchCommands),
				input);

			input = new InputGestureCollection
			{
				new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D")
			};

			Delete = new RoutedUICommand(
				"Delete mapping",
				nameof(Delete),
				typeof(SwitchCommands),
				input);

			Show = new RoutedUICommand(
				"Show", nameof(Show), typeof(SwitchCommands));
		}
	}
}
