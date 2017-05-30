using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace KeyboardSwitch.Services
{
	public static class FileManager
	{
		public static readonly string SettingsLocation =
			Path.Combine(
				Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
					?? Environment.CurrentDirectory,
				"mappings");

		public static bool TryRead(
			out Dictionary<CultureInfo, StringBuilder> dict,
			bool all = false)
		{
			dict = null;

			var langs = InputLanguageManager.Current.AvailableInputLanguages
				?.Cast<CultureInfo>()
				.ToList();

			bool result = false;

			if (langs == null)
			{
				return false;
			}

			dict = new Dictionary<CultureInfo, StringBuilder>();

			StreamReader reader = null;

			try
			{
				reader = new StreamReader(SettingsLocation, Encoding.UTF8);

				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();

					if (String.IsNullOrEmpty(line))
					{
						continue;
					}

					var tokens = line.Split('\t');
					CultureInfo lang = null;

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
							dict.Add(lang, new StringBuilder(tokens[1]));
						}
					} else
					{
						dict.Add(lang, new StringBuilder(tokens[1]));
					}
				}
				
				foreach (var lang in langs)
				{
					if (!dict.ContainsKey(lang))
					{
						dict.Add(lang, new StringBuilder());
					}
				}

				int maxLength =
					dict.Values
						.Select(str => str.Length)
						.Concat(new[] { 0 })
						.Max();

				foreach (var str in dict.Values)
				{
					str.Append(new String(' ', maxLength - str.Length));
				}

				result = true;
			} catch
			{
				dict = langs.ToDictionary(
					lang => lang,
					lang => new StringBuilder(" "));
				result = false;
			} finally
			{
				reader?.Close();
			}

			return result;
		}

		public static bool TryWrite(
			Dictionary<CultureInfo, StringBuilder> dict)
		{
			bool result = TryRead(out var fullDict, true);

			if (!result)
			{
				return false;
			}

			StreamWriter writer = null;

			try
			{
				writer = new StreamWriter(
					SettingsLocation,
					false,
					Encoding.UTF8);

				foreach (var pair in fullDict)
				{
					writer.WriteLine(
						dict.ContainsKey(pair.Key)
							? $"{pair.Key}\t{dict[pair.Key]}"
							: $"{pair.Key}\t{pair.Value}");
				}

				result = true;
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
