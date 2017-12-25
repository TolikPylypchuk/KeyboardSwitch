using System.Windows;
using System.Windows.Controls;

using KeyboardSwitch.Services;

namespace KeyboardSwitch.UI
{
	public class SettingsViewModel : DependencyObject
	{
		public static readonly DependencyProperty CanSaveProperty;

		static SettingsViewModel()
		{
			CanSaveProperty = DependencyProperty.Register(
				nameof(CanSave),
				typeof(bool),
				typeof(SettingsViewModel));
		}

		public SettingsViewModel(
			App currentApp,
			LanguageManager languageManager,
			ITextManager defaultTextManager)
		{
			this.CurrentApp = currentApp;
			this.LanguageManager = languageManager;
			this.DefaultTextManager = defaultTextManager;
		}

		public App CurrentApp { get; }
		public LanguageManager LanguageManager { get; }
		public ITextManager DefaultTextManager { get; }

		public bool CanSave
		{
			get => (bool)this.GetValue(CanSaveProperty);
			set => this.SetValue(CanSaveProperty, value);
		}

		public bool CanClose { get; set; }
		public bool IsBorderClicked { get; set; }
		public Border FocusedBorder { get; set; }
	}
}
