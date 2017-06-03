using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using KeyboardSwitch.Services;

namespace KeyboardSwitch.Tests.Services
{
	[TestClass]
	public class LanguageManagerTests
	{
		private CultureInfo enUS = new CultureInfo("en-US");
		private CultureInfo ukUA = new CultureInfo("uk-UA");

		private CultureInfo currentLang;

		private Mock<IInputLanguageManager> mockInputLanguageManager;
		private Mock<ILayoutManager> mockLayoutManager;
		private Mock<ITextManager> mockTextManager;

		private string text;

		[TestInitialize]
		public void Initialize()
		{
			mockInputLanguageManager = new Mock<IInputLanguageManager>();

			mockInputLanguageManager.SetupGet(ls => ls.InputLanguages)
				.Returns(new[] { ukUA, enUS });

			LanguageManager.Current.InputLanguageManager =
				mockInputLanguageManager.Object;
			
			LanguageManager.Current.Languages =
				new Dictionary<CultureInfo, StringBuilder>
			{
				[enUS] = new StringBuilder("as"),
				[ukUA] = new StringBuilder("фі")
			};

			mockLayoutManager = new Mock<ILayoutManager>();

			mockLayoutManager.Setup(m => m.SetCurrentLayout(ukUA))
				.Callback(() => currentLang = ukUA);
			mockLayoutManager.Setup(m => m.SetCurrentLayout(enUS))
				.Callback(() => currentLang = enUS);

			LanguageManager.Current.LayoutManager = mockLayoutManager.Object;

			mockTextManager = new Mock<ITextManager>();

			LanguageManager.Current.TextManager = mockTextManager.Object;
		}

		[TestMethod]
		public void SwitchTextForwardTest()
		{
			currentLang = enUS;

			mockLayoutManager.Setup(m => m.GetCurrentLayout())
				.Returns(currentLang);

			text = "as";

			mockTextManager.SetupGet(m => m.HasText)
				.Returns(true);
			mockTextManager.Setup(m => m.GetText())
				.Returns(text);
			mockTextManager.Setup(m => m.SetText("as"))
				.Callback(() => text = "as");
			mockTextManager.Setup(m => m.SetText("фі"))
				.Callback(() => text = "фі");

			LanguageManager.Current.SetCurrentLanguage();
			LanguageManager.Current.SwitchText(true);

			Assert.AreEqual("фі", text);

			mockTextManager.Verify(m => m.GetText(), Times.Once());
			mockTextManager.Verify(m => m.SetText("фі"), Times.Once());
		}
	}
}
