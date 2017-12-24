using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace KeyboardSwitch.Services
{
	public class FileManager
	{
		public FileManager(string mappingsLocation)
		{
			this.MappingsLocation = mappingsLocation;
		}
		
		public string MappingsLocation { get; }

		public Dictionary<CultureInfo, StringBuilder> Read(bool all = false)
		{
			var langs = InputLanguageManager.Current.AvailableInputLanguages
				?.Cast<CultureInfo>()
				.ToList();
			
			if (langs == null)
			{
				return null;
			}

			var result = new Dictionary<CultureInfo, StringBuilder>();

			StreamReader reader = null;

			try
			{
				reader = new StreamReader(this.MappingsLocation, Encoding.UTF8);

				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();

					if (String.IsNullOrEmpty(line))
					{
						continue;
					}

					var tokens = line.Split('\t');
					CultureInfo lang;

					try
					{
						lang = new CultureInfo(tokens[0]);
					} catch
					{
						continue;
					}

					if (!all)
					{
						if (langs.Contains(lang))
						{
							result.Add(lang, new StringBuilder(tokens[1]));
						}
					} else
					{
						result.Add(lang, new StringBuilder(tokens[1]));
					}
				}
				
				foreach (var lang in langs)
				{
					if (!result.ContainsKey(lang))
					{
						result.Add(lang, new StringBuilder());
					}
				}

				int maxLength =
					result.Values
						.Select(str => str.Length)
						.Concat(new[] { 0 })
						.Max();

				foreach (var str in result.Values)
				{
					if (str.Length != maxLength)
					{
						str.Append(new String(' ', maxLength - str.Length));
					}
				}
			} catch
			{
				result = null;
			} finally
			{
				reader?.Close();
			}

			return result;
		}

		public bool Write(Dictionary<CultureInfo, StringBuilder> dict)
		{
			var fullDict = this.Read(true);

			if (fullDict == null)
			{
				return false;
			}

			StreamWriter writer = null;
			bool result = true;

			try
			{
				writer = new StreamWriter(
					this.MappingsLocation,
					false,
					Encoding.UTF8);

				foreach (var pair in fullDict)
				{
					writer.WriteLine(
						dict.ContainsKey(pair.Key)
							? $"{pair.Key}\t{dict[pair.Key]}"
							: $"{pair.Key}\t{pair.Value}");
				}
			} catch
			{
				result = false;
			} finally
			{
				writer?.Close();
			}

			return result;
		}
	}
}
