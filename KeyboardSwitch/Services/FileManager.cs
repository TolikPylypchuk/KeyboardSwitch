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
	public class FileManager
	{
		public FileManager(string mappingsFile)
		{
			this.DefaultMappingsFile = mappingsFile
				?? throw new ArgumentNullException(nameof(mappingsFile));

			this.CurrentMappingsFile = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				Assembly.GetExecutingAssembly().GetName().Name,
				Path.GetFileName(mappingsFile));
		}
		
		public string DefaultMappingsFile { get; }
		public string CurrentMappingsFile { get; }

		public Dictionary<CultureInfo, StringBuilder> Read(bool all = false)
		{
			var langs = InputLanguageManager.Current.AvailableInputLanguages
				?.Cast<CultureInfo>()
				.ToList();
			
			if (langs == null)
			{
				return null;
			}

			this.CreateFilesIfTheyDontExist();

			var result = new Dictionary<CultureInfo, StringBuilder>();

			StreamReader reader = null;

			try
			{
				reader = new StreamReader(this.CurrentMappingsFile, Encoding.UTF8);
				
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

		public bool Write(
			Dictionary<CultureInfo, StringBuilder> newMappings,
			bool toDefault = false)
		{
			var oldMappings = this.Read(true);

			if (oldMappings == null)
			{
				return false;
			}

			StreamWriter writer = null;
			bool result = true;

			try
			{
				writer = new StreamWriter(
					toDefault ? this.DefaultMappingsFile : this.CurrentMappingsFile,
					false,
					Encoding.UTF8);

				foreach (var pair in oldMappings)
				{
					writer.WriteLine(
						newMappings.ContainsKey(pair.Key)
							? $"{pair.Key}\t{newMappings[pair.Key]}"
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

		private void CreateFilesIfTheyDontExist()
		{
			if (!File.Exists(this.DefaultMappingsFile))
			{
				File.Create(this.DefaultMappingsFile).Close();
			}

			string dir = Path.GetDirectoryName(this.CurrentMappingsFile);

			if (dir != null && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			if (!File.Exists(this.CurrentMappingsFile))
			{
				File.WriteAllText(
					this.CurrentMappingsFile,
					File.ReadAllText(this.DefaultMappingsFile));
			}
		}
	}
}
