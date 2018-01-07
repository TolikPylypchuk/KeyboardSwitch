using System.Windows.Input;

namespace KeyboardSwitch.UI
{
	public static class SwitchCommands
	{
		public static RoutedUICommand Forward { get; }
		public static RoutedUICommand Backward { get; }

		public static RoutedUICommand SaveAsDefault { get; }

		public static RoutedUICommand MoveRight { get; }
		public static RoutedUICommand MoveLeft { get; }

		public static RoutedUICommand Delete { get; }

		public static RoutedUICommand Show { get; }

		static SwitchCommands()
		{
			Forward = new RoutedUICommand(
				"Forward",
				nameof(Forward),
				typeof(SwitchCommands),
				new InputGestureCollection
				{
					new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F")
				});
			
			Backward = new RoutedUICommand(
				"Backward",
				nameof(Backward),
				typeof(SwitchCommands),
				new InputGestureCollection
				{
					new KeyGesture(Key.B, ModifierKeys.Control, "Ctrl+B")
				});

			SaveAsDefault = new RoutedUICommand(
				"Save as default",
				nameof(SaveAsDefault),
				typeof(SwitchCommands),
				new InputGestureCollection
				{
					new KeyGesture(
						Key.S, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+S")
				});

			MoveRight = new RoutedUICommand(
				"Move right",
				nameof(MoveRight),
				typeof(SwitchCommands),
				new InputGestureCollection
				{
					new KeyGesture(Key.Right, ModifierKeys.Alt, "Alt+Right")
				});

			MoveLeft = new RoutedUICommand(
				"Move left",
				nameof(MoveLeft),
				typeof(SwitchCommands),
				new InputGestureCollection
				{
					new KeyGesture(Key.Left, ModifierKeys.Alt, "Alt+Left")
				});

			Delete = new RoutedUICommand(
				"Delete mapping",
				nameof(Delete),
				typeof(SwitchCommands),
				new InputGestureCollection
				{
					new KeyGesture(Key.Delete)
				});

			Show = new RoutedUICommand(
				"Show", nameof(Show), typeof(SwitchCommands));
		}
	}
}
