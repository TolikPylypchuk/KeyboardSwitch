using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace KeyboardSwitch
{
	static class FileManager
	{
		public static readonly string SettingsLocation =
			Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
			"mappings");

		public static Dictionary<CultureInfo, StringBuilder> Read(out bool success,
			bool all = false)
		{
			var retVal = new Dictionary<CultureInfo, StringBuilder>();

			try
			{
				using (StreamReader reader =
					new StreamReader(SettingsLocation, Encoding.UTF8))
				{
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();
						if (line.Equals(string.Empty))
						{
							continue;
						}
						string[] strs = line.Split('\t');
						CultureInfo lang = null;
						try
						{
							lang = new CultureInfo(strs[0]);
						} catch
						{
							continue;
						}
						if (!all)
						{
							if ((InputLanguageManager.Current.AvailableInputLanguages
								as ArrayList).Contains(lang))
							{
								retVal.Add(lang, new StringBuilder(strs[1]));
							}
						} else
						{
							retVal.Add(lang, new StringBuilder(strs[1]));
						}
					}

					foreach (var lang in InputLanguageManager.Current.
						AvailableInputLanguages)
					{
						CultureInfo info = lang as CultureInfo;
						if (!retVal.ContainsKey(info))
						{
							retVal.Add(info, new StringBuilder());
						}
					}

					int maxLength = 0;
					foreach (var str in retVal.Values)
					{
						if (str.Length > maxLength)
						{
							maxLength = str.Length;
						}
					}

					foreach (var str in retVal.Values)
					{
						while (str.Length < maxLength)
						{
							str.Append(' ');
						}
					}
				}
				success = true;
			} catch
			{
				retVal = new Dictionary<CultureInfo, StringBuilder>();
				foreach (var lang in InputLanguageManager.Current.
					AvailableInputLanguages)
				{
					retVal.Add(lang as CultureInfo, new StringBuilder(" "));
				}
				success = false;
			}

			return retVal;
		}

		public static void Write(Dictionary<CultureInfo, StringBuilder> dict,
			out bool success)
		{
			bool successRead = false;
			Dictionary<CultureInfo, StringBuilder> fullDict = Read(out successRead, true);

			try
			{
				using (StreamWriter writer =
					new StreamWriter(SettingsLocation, false, Encoding.UTF8))
				{
					foreach (var pair in fullDict)
					{
						if (dict.ContainsKey(pair.Key))
						{
							writer.Write(pair.Key.ToString() + "\t");
							writer.WriteLine(dict[pair.Key]);
						} else
						{
							writer.Write(pair.Key.ToString() + "\t");
							writer.WriteLine(pair.Value);
						}
					}
				}
				success = true;
			} catch
			{
				success = false;
			}
		}
	}
}
