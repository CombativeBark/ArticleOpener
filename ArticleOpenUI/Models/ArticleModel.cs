using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using HtmlAgilityPack;

namespace ArticleOpenUI.Models
{
	enum ArticleType
	{
		None,
		Tool,
		Modification,
		Plastic,
		PlasticVariant
	}

	class ArticleModel
	{
		private string _name = "";
		private ArticleType _type = ArticleType.None;
		private string _path;
		private Uri _uri;

		public string Name
		{
			get => _name;
			set
			{
				if (CheckNameValidity(value))
					_name = value;
			}
		}
		public ArticleType Type { get; set; }
		public string Path { get; set; }
		public Uri URI { get; set; }

		private ArticleModel(string name)
		{
			Name = name;
			Type = FindArticleType();
			Path = FindPath();
			URI = FindURI();
		}

		public static ArticleModel? CreateArticle(string name)
		{
			try
			{
				return new ArticleModel(name);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return null;
			}
        }

		private bool CheckNameValidity(string name)
		{
			const string REGEX_ARTICLE = @"^\d{6}[VP][1-9]?(-\d)?$";
			if (Regex.IsMatch(name, REGEX_ARTICLE, RegexOptions.Compiled))
				return true;
			else
			{
				throw new ArgumentException($"Input \"{name}\" isn't a valid article");
			}
			

		}

		public List<string> GetChildren()
		{
			List<string> result = new();
			var html = new HtmlDocument();
			var web = new HtmlWeb();

			html = web.Load(URI);

			if (html.ParseErrors != null && html.ParseErrors.Any())
			{
				throw new HtmlWebException($"{html.ParseErrors.Count()} errors occured while trying to parse HTML");
			}
			else
			{

				if (html.DocumentNode != null)
				{
					HtmlNode bodyNode = html.DocumentNode.SelectSingleNode("//body");

					if (bodyNode != null)
					{
						var children = bodyNode.SelectNodes("//td/a")
							.Where(x => Regex.IsMatch(x.Attributes["href"].Value, @"^(?:plastic/)\d+P(?:-\d)?$"));

						foreach (var child in children)
						{
							string childPlastic = child.Attributes["href"].Value.Replace("plastic/", "");
							result.Add(childPlastic);
						}

					}
				}
			}
			return result;
		}

		private ArticleType FindArticleType()
		{
			const string REGEX_PLASTIC = @"^\d{6}P(-\d)?$";
			const string REGEX_PLASTIC_VARIANT = @"^\d{6}P-\d$";
			const string REGEX_TOOL = @"^\d{6}V[1-9]?$";
			const string REGEX_MODIFICATION = @"^\d{6}V\d$";

			if (Regex.IsMatch(Name, REGEX_PLASTIC, RegexOptions.Compiled))
			{

				if (Regex.IsMatch(Name, REGEX_PLASTIC_VARIANT, RegexOptions.Compiled))
					return ArticleType.PlasticVariant;
				else
					return ArticleType.Plastic;
			}
			else if (Regex.IsMatch(Name, REGEX_TOOL, RegexOptions.Compiled))
			{
				if (Regex.IsMatch(Name, REGEX_MODIFICATION, RegexOptions.Compiled))
					return ArticleType.Modification;
				else
					return ArticleType.Tool;
			}
			else
				throw new ArgumentException("Article type could not be identified", "Type");
		}

		private string FindPath()
		{
			string rootPath = @"\\Server1\ArtikelFiler\ArticleFiles";
			string path = $@"{rootPath}\{Name}\{Name}";
			string fullPath = path;
			char[] trimNumbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };


			if (Type == ArticleType.Modification)
			{
				path = $@"{rootPath}\{Name.TrimEnd(trimNumbers)}";
				fullPath = $@"{path}\{Name}";
			}
			else if (Type == ArticleType.PlasticVariant)
			{
				path = $@"{rootPath}\{Name}";
				fullPath = path;
			}

			if (Directory.Exists(fullPath))
				return path;
			else
				throw new ArgumentException($"Directory for {Name} doesn't exist");
		}

		public void OpenFolder()
		{
			if (string.IsNullOrEmpty(Path))
				throw new ArgumentNullException("Path", "Path is not assigned");

			if (Directory.Exists(Path))
			{
				ProcessStartInfo startInfo = new()
				{
					Arguments = Path,
					FileName = "explorer.exe"
				};

				Process.Start(startInfo);
			}
			else
				throw new ArgumentException($"Can't open directory for article {Name} as it doesn't exist");
			
		}

		private Uri FindURI()
		{
			string baseURL = @"http://server1:85";
			string type = "tool";

			if (Type == ArticleType.None)
				throw new ArgumentException("No URI for Article type of None", "Type");

			if (Type == ArticleType.Plastic || Type == ArticleType.PlasticVariant)
				type = "plastic";

			return new Uri($"{baseURL}/{type}/{Name}");
		}

		public void OpenInfo()
		{
			if (URI == null)
				throw new ArgumentNullException("URI", $"{Name} has no valid Uri");

			ProcessStartInfo startInfo = new()
			{
				FileName = URI.AbsoluteUri,
				UseShellExecute = true,
			};
			Process.Start(startInfo);
		}

		public void OpenAll()
		{
			OpenInfo();
			OpenFolder();
			Thread.Sleep(50);
		}

		public void PrintInfo()
		{
			MessageBox.Show($"Article Info\n\tName: {Name}\n\tType: {Type}\n\tPath: {Path}\n\tURL: {URI}");
		}
	}
}
