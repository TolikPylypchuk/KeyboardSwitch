using System.Windows.Input;

namespace KeyboardSwitch
{
	static class SwitchCommands
	{
		private static RoutedUICommand forward;
		private static RoutedUICommand backward;
		private static RoutedUICommand delete;
		private static RoutedUICommand show;

		public static RoutedUICommand Forward { get { return forward; } }
		public static RoutedUICommand Backward { get { return backward; } }
		public static RoutedUICommand Delete { get { return delete; } }
		public static RoutedUICommand Show { get { return show; } }

		static SwitchCommands()
		{
			InputGestureCollection input = new InputGestureCollection()
			{
				new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F")
			};
			forward = new RoutedUICommand("Forward", "Forward",
				typeof(SwitchCommands), input);

			input = new InputGestureCollection()
			{
				new KeyGesture(Key.B, ModifierKeys.Control, "Ctrl+B")
			};
			backward = new RoutedUICommand("Backward", "Backward",
				typeof(SwitchCommands), input);

			input = new InputGestureCollection()
			{
				new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D")
			};
			delete = new RoutedUICommand("Delete mapping", "DeleteMapping",
				typeof(SwitchCommands), input);

			show = new RoutedUICommand("Show", "Show", typeof(SwitchCommands));
		}
	}
}
