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
			this.mockInputLanguageManager = new Mock<IInputLanguageManager>();

			this.mockInputLanguageManager.SetupGet(ls => ls.InputLanguages)
				.Returns(new[] { this.ukUA, this.enUS });

			LanguageManager.Current.InputLanguageManager =
				this.mockInputLanguageManager.Object;
			
			LanguageManager.Current.Languages =
				new Dictionary<CultureInfo, StringBuilder>
			{
				[this.enUS] = new StringBuilder("as"),
				[this.ukUA] = new StringBuilder("фі")
			};

			this.mockLayoutManager = new Mock<ILayoutManager>();

			this.mockLayoutManager.Setup(m => m.SetCurrentLayout(this.ukUA))
				.Callback(() => currentLang = this.ukUA);
			this.mockLayoutManager.Setup(m => m.SetCurrentLayout(this.enUS))
				.Callback(() => currentLang = this.enUS);

			LanguageManager.Current.LayoutManager = this.mockLayoutManager.Object;

			this.mockTextManager = new Mock<ITextManager>();

		}

		[TestMethod]
		public void SwitchTextForwardTest()
		{
			this.currentLang = this.enUS;

			this.mockLayoutManager.Setup(m => m.GetCurrentLayout())
				.Returns(this.currentLang);

			this.text = "as";

			this.mockTextManager.SetupGet(m => m.HasText)
				.Returns(true);
			this.mockTextManager.Setup(m => m.GetText())
				.Returns(this.text);
			this.mockTextManager.Setup(m => m.SetText("as"))
				.Callback(() => this.text = "as");
			this.mockTextManager.Setup(m => m.SetText("фі"))
				.Callback(() => this.text = "фі");

			LanguageManager.Current.SetCurrentLanguage();
			LanguageManager.Current.SwitchText(this.mockTextManager.Object, true);

			Assert.AreEqual("фі", this.text);

			this.mockTextManager.Verify(m => m.GetText(), Times.AtLeastOnce());
			this.mockTextManager.Verify(m => m.SetText("фі"), Times.AtLeastOnce());
		}

		[TestMethod]
		public void SwitchTextBackwardTest()
		{
			this.currentLang = this.ukUA;

			this.mockLayoutManager.Setup(m => m.GetCurrentLayout())
				.Returns(this.currentLang);

			this.text = "фі";

			this.mockTextManager.SetupGet(m => m.HasText)
				.Returns(true);
			this.mockTextManager.Setup(m => m.GetText())
				.Returns(this.text);
			this.mockTextManager.Setup(m => m.SetText("as"))
				.Callback(() => this.text = "as");
			this.mockTextManager.Setup(m => m.SetText("фі"))
				.Callback(() => this.text = "фі");

			LanguageManager.Current.SetCurrentLanguage();
			LanguageManager.Current.SwitchText(this.mockTextManager.Object, false);

			Assert.AreEqual("as", this.text);

			this.mockTextManager.Verify(m => m.GetText(), Times.AtLeastOnce());
			this.mockTextManager.Verify(m => m.SetText("as"), Times.AtLeastOnce());
		}
	}
}
