using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
	public class ArticleInfo
	{
		private string m_FullPath { get; set; } = "";
		private bool m_IsModification { get; set; } = false;
		private bool m_IsVariant { get; set; } = false;

		public string Name { get; private set; } = "";
		public string Path { get; private set; } = "";
		public string URL { get; private set; } = "";
		public List<string>? Plastics { get; private set; } = null;
		public string CAD { get; private set; } = "Unknown";
		public string Customer { get; private set; } = "Unknown";
		public string Description { get; private set; } = "Unknown";
		public string Material { get; private set; } = "Unknown";
		public string Shrinkage { get; private set; } = "";
		public string Machine { get; private set; } = "Unknown";
		public ArticleType Type { get; private init; }

		public ArticleInfo(string name)
		{
			if (IsNameValid(name))
				Name = name;
			else
				throw new ArgumentException($"{name} is not a valid article.");

			Type = GetArticleType();
			Path = GetPath();
			URL = GetURL();

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

			if (Regex.IsMatch(Name, toolPattern[0]))
			{
				if (Regex.IsMatch(Name, toolPattern[1]))
					m_IsModification = true;

				return ArticleType.Tool;
			}
			else if (Regex.IsMatch(Name, plasticPattern[0]))
			{
				if (Regex.IsMatch(Name, plasticPattern[1]))
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
			HtmlDocument doc;
			var web = new HtmlWeb();

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			web.AutoDetectEncoding = false;
			web.OverrideEncoding = Encoding.UTF8;

			doc = web.Load(URL);
			if (doc == null || web.StatusCode != System.Net.HttpStatusCode.OK)
				throw new HtmlWebException($"Couldn't load web page for {Name}.");

			var divContent = doc.DocumentNode
				.SelectSingleNode("//body/div[@class='page']/div[@class='main']/div[@class='content px-4']");

			if (divContent == null)
				throw new NodeNotFoundException($"Couldn't find parse web page for {Name}");

			var filteredList = divContent.ChildNodes
				.Where(x => x.Name != "#text")
				.ToList();

			for (int i = 1; i < filteredList.Count; i++)
			{
				var currentNode = filteredList[i];
				if (currentNode.Name == "table")
					continue;

				var nextNode = filteredList[i + 1];

				switch (currentNode.InnerText.ToLower())
				{
					case ("customer"):
						var customerNode = nextNode.SelectNodes(".//td")[0];
						Customer = WebUtility.HtmlDecode(customerNode?.InnerText) ?? "";
						var descriptionNode = filteredList[i + 2].SelectNodes(".//td")[0];
						Description = WebUtility.HtmlDecode(descriptionNode?.InnerText) ?? "";
						break;
					case ("notes"):
						ProcessProjectNotes(nextNode);
						break;
					case ("material"):
						ProcessMaterialNode(nextNode);
						break;
					case ("operations"):
						ProcessOperations(nextNode);
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

		private void ProcessOperations(HtmlNode rootNode)
		{
			Regex regex = new Regex(@"^21[1-9]0$", RegexOptions.Compiled);

			foreach (var child in rootNode.ChildNodes[2].ChildNodes)
			{
				if (!regex.IsMatch(child.ChildNodes[0].InnerText))
					continue;

				Machine = WebUtility.HtmlDecode(child.ChildNodes[2].InnerText);
				break;
			}
		}

		private void ProcessMaterialNode(HtmlNode rootNode)
		{
			if (Type != ArticleType.Plastic)
				return;

			// TODO: Fix, Then improve RegEx
			Regex regex = new Regex(@"^\d{6}(?:-\d)?\s*-\s*(?<Material>.*?)\s?(?:(?<Shrinkage>\b\d(?:[,.]\d+)?%)|[Xx]%)?", RegexOptions.Compiled);

			// TODO: Improve Readability
			foreach (var rootChild in rootNode.ChildNodes[2].ChildNodes)
			{
				var nodes = rootChild.ChildNodes;
				if (!nodes.Any() || nodes[0].InnerText.ToLower() != "plastic")
					continue;
				var data = nodes[2]?.InnerText;
				if (data == null)
					return;

				var decodedData = WebUtility.HtmlDecode(data);
				var regExResult = regex.Match(decodedData);
				if (!regExResult.Success)
					return;

				var materialCapture = regExResult.Groups["Material"];
				if (materialCapture.Success)
					Material = materialCapture.Value;

				if (regExResult.Groups.Count < 2)
					return;

				var shrinkageCapture = regExResult.Groups["Shrinkage"];
				if (shrinkageCapture.Success)
					Shrinkage = shrinkageCapture.Value;

				break;
			}
		}

		private void ProcessProjectNotes(HtmlNode rootNode)
		{
			if (Type != ArticleType.Tool)
				return;

			var data = rootNode.SelectNodes(".//td")[1].InnerText;
			var decodedData = WebUtility.HtmlDecode(data);
			var regExResults = Regex.Match(decodedData, @"\d{6} (?<CAD>\w+) // (?<Shrinkage>[Kk]rymp\s*\d(?:[,.]\d+)?%)");

			if (!regExResults.Success ||
				regExResults.Groups.Count < 1)
				return;

			var cadOperator = regExResults.Groups["CAD"];
			if (cadOperator.Success &&
				!cadOperator.Value.Equals("Andreas"))
				CAD = cadOperator.Value;

			var shrinkageCapture = regExResults.Groups["Shrinkage"];
			if (shrinkageCapture.Success)
				Shrinkage = shrinkageCapture.Value
					.ToLower()
					.Replace("krymp ", "");
		}

		private List<string> GetPlasticsFromNode(HtmlNode rootNode)
		{
			var output = new List<string>();

			var tdNodes = rootNode.SelectNodes(".//td");
			for (int i = 0; i < tdNodes?.Count; i++)
			{
				if (i % 3 == 0 && IsPlasticsNode(tdNodes[i]))
					output.Add(tdNodes[i].InnerText);
			}

			return output;
		}
		private bool IsPlasticsNode(HtmlNode node)
		{
			bool output = Regex.IsMatch(node.InnerText, @"^\d{6}P(?:-\d)?$");
			return output;
		}
	}
}