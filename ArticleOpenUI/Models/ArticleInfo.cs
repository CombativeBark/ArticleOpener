using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
	public class ArticleInfo
	{
		public string Name { get; private set; } = "Unknown";
		public string URL { get; private set; } = "Unknown";
		public string CAD { get; private set; } = "Unknown";
		public string Customer { get; private set; } = "Unknown";
		public string Description { get; private set; } = "Unknown";
		public string Material { get; private set; } = "Unknown";
		public string Shrinkage { get; private set; } = "Unknown";
		public string Machine { get; private set; } = "Unknown";
		public List<string>? Plastics { get; private set; } = null;
		public ArticleType Type { get; private init; }

		public ArticleInfo(string name)
		{

			Name = name;
			Type = GetArticleType();
			URL = GetURL();

			if (URL != null)
				PullFromWeb();
		}

		private bool IsNameValid(string name)
		{
			if (name == null || string.IsNullOrWhiteSpace(name))
				return false;
			return true;
		}
		private ArticleType GetArticleType()
		{
			string[] toolPattern =
			{
				@"^\d{6}V\d?$",
				@"^\d{6}V\d$"
			};
			string[] plasticPattern =
			{
				@"^\d{6}P(?:-\d)?$",
				@"^\d{6}P-\d$"
			};

			if (Regex.IsMatch(Name, toolPattern[0], RegexOptions.Compiled))
			{
				if (Regex.IsMatch(Name, toolPattern[1], RegexOptions.Compiled))
					m_IsModification = true;

				return ArticleType.Tool;
			}
			else if (Regex.IsMatch(Name, plasticPattern[0], RegexOptions.Compiled))
			{
				if (Regex.IsMatch(Name, plasticPattern[1], RegexOptions.Compiled))
					m_IsVariant = true;
				return ArticleType.Plastic;
			}
			else
			{
				throw new ArgumentException($"Couldn't find type for article {Name}");
			}
		}
		private string GetPath()
		{
			var basePath = $@"\\server1\ArtikelFiler\ArticleFiles\{Name}";
			if (m_IsModification)
				basePath = basePath.Substring(0, basePath.Length - 1);
			else if (m_IsVariant)
				basePath = basePath.Substring(0, basePath.Length - 2);

			if (Type == ArticleType.Tool || Type == ArticleType.Plastic)
			{
				m_FullPath = basePath + @"\" + Name;

				if (!Directory.Exists(m_FullPath))
					throw new DirectoryNotFoundException($"Directory for {Name} doesn't exist.");

				if (m_IsModification || m_IsVariant)
					return basePath;

				return m_FullPath;
			}

			throw new ArgumentException($"{Name} doesn't have a corresponding path.");
		}
		private string GetURL()
		{
			switch (Type)
			{
				case ArticleType.Tool:
					return $@"http://server1:85/tool/{Name}";
				case ArticleType.Plastic:
					return $@"http://server1:85/plastic/{Name}";
				default:
					throw new ArgumentException($"Article type of {Name} is not support");
			}
		}
		private void PullFromWeb()
		{
			var doc = new HtmlDocument();
			var web = new HtmlWeb();

			doc = web.Load(URL);
			if (doc == null)
				throw new HtmlWebException("Couldn't load web page.");

			var divContent = doc.DocumentNode
				.SelectSingleNode("//body/div[@class='page']/div[@class='main']/div[@class='content px-4']");

			if (divContent == null)
				throw new NodeNotFoundException();

			var filteredList = divContent.ChildNodes
				.Where(x => x.Name != "#text")
				.ToList();

			for (int i = 1; i < filteredList.Count; i++)
			{
				var node = filteredList[i];
				if (node.Name == "table")
					continue;

				var nextNode = filteredList[i + 1];

				switch (node.InnerText.ToLower())
				{
					case ("customer"):
						Customer = nextNode.SelectNodes(".//td")[0].InnerText;
						Description = filteredList[i + 2].SelectNodes(".//td")[0].InnerText;
						break;
					case ("notes"):
						if (Type == ArticleType.Tool)
						{
							var data = nextNode.SelectNodes(".//td")[1].InnerText;
							var match = Regex.Match(data, @".*\d{6} (\w+) // (Krymp \d[,.]\d+%).*");

							if (match.Success)
							{ 
								CAD = match.Groups[1].Value;
								Shrinkage = match.Groups[2].Value;
							}
						}
						break;
					case ("material"):
						if (Type == ArticleType.Plastic)
						{
							foreach (var child in nextNode.ChildNodes[2].ChildNodes)
							{
								if (child.ChildNodes.Any() && child.ChildNodes[0].InnerText.ToLower() == "plastic")
								{
									var matches = Regex.Match(child.ChildNodes[2].InnerText, @"^\d{6} - (.*)(?: (\d[,.]\d+%)| [Xx]%)?$", RegexOptions.Compiled);
									if (matches.Success && matches.Groups[1].Success)
										Material = matches.Groups[1].Value.Replace("&#x2122", "™");

									if (matches.Groups.Count > 2 && matches.Groups[2].Success)
										Shrinkage = matches.Groups[2].Value;
								}
							}
						}
						break;
					case ("operations"):
						foreach (var child in nextNode.ChildNodes[2].ChildNodes)
						{
							if (child.ChildNodes[0].InnerText == "2120" || child.ChildNodes[0].InnerText == "2110")
							{
								Machine = child.ChildNodes[2].InnerText;
								break;
							}
						}
						break;
					case ("plastics"):
						if (Type == ArticleType.Tool)
							Plastics = GetPlasticsFromNode(nextNode);
						break;
					default:
						break;
				}
			}
		}
		private List<string> GetPlasticsFromNode(HtmlNode node)
		{
			var output = new List<string>();

			var tdNodes = node.SelectNodes(".//td");
			for (int i = 0; i < tdNodes.Count(); i++)
			{
				if (i % 3 == 0 && IsPlasticsNode(tdNodes[i]))
					output.Add(tdNodes[i].InnerText);
			}

			return output;
		}
		private bool IsPlasticsNode(HtmlNode node)
		{
			const string plasticPattern = @"^\d{6}P(?:-\d)?$";
			var output = Regex.IsMatch(node.InnerText, plasticPattern, RegexOptions.Compiled);
			return output;
		}
	}
}