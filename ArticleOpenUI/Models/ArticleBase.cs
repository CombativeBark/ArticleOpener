using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
	internal abstract class ArticleBase : IArticle
	{
		public string Name { get; set; }
		public ArticleType Type { get; set; }
		public string Path { get; set; }
		public string Url { get; set; }

		public abstract IArticle CreateArticle(string name);

		public abstract List<IArticle> GetChildren();

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


		private protected bool IsNameValid(string name)
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

		private protected abstract string GetPath();

		private protected abstract string GetUrl();
	}
}
