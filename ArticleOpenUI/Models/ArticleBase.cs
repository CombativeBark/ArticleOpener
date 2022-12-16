using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
	public abstract class ArticleBase
	{
		private string _name = "";
		private ArticleType _type;

		public virtual string Name { get => _name; set => _name = value; }
		public virtual ArticleType Type { get => _type; private set => _type = value; }
		public abstract string Path { get; }
		public abstract string Url { get; }

		public abstract List<PlasticArticle> GetChildren();

		public void OpenFolder()
		{
			if (Directory.Exists(Path))
			{
				ProcessStartInfo startInfo = new ProcessStartInfo()
				{
					Arguments = Path,
					FileName = "explorer.exe"
				};

				Process.Start(startInfo);
			}
			else
			{
				throw new Exception($"Error: Directory for Article {Name} doesn't exist");
			}
		}

		public void OpenInfo()
		{
			ProcessStartInfo startInfo = new()
			{
				FileName = Url,
				UseShellExecute = true,
			};
			Process.Start(startInfo);
		}

		private bool IsNameValid(string name)
		{
			const string regex = @"^\d{6}[VP](?:\d|-\d)?$";

			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Error: Article Number is null or empty.");
			}

			if (Regex.IsMatch(name, regex, RegexOptions.Compiled))
			{
				return true;
			}
			else
			{
				throw new ArgumentException($"Error: {name} is not a valid Article Number");
			}
		}
	}
}
