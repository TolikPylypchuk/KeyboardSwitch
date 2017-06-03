namespace KeyboardSwitch.Services
{
	public interface ITextManager
	{
		bool HasText { get; }

		string GetText();
		void SetText(string text);
	}
}
