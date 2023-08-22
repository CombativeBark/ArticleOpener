using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace ArticleOpenUI.Models
{
	public class ArticleInfo
	{
		public string Name { get; set; } = "";
		public ArticleType Type { get; set; }
		public bool IsModOrVariant { get; set; } = false;
		public string Url { get; set; } = "";
		public List<ArticleInfo> Plastics { get; set; } = new List<ArticleInfo>();
		public string CAD { get; set; } = "Unknown";
		public string Customer { get; set; } = "Unknown";
		public string Description { get; set; } = "Unknown";
		public string Material { get; set; } = "Unknown";
		public Dictionary<string, string> Shrinkage { get; set; } = new Dictionary<string, string>();
		public string Machine { get; set; } = "Unknown";

		public ArticleInfo()
		{
			MessageBox.Show("Created without an article number!", "ArticleInfo", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		public ArticleInfo(string name)
		{
			if (IsNameValid(name))
				Name = name;
			else
				throw new ArgumentException($"{name} is not a valid article.");

			Type = ResolveType();
			Url = GenerateUrl();

			PullFromWeb();
			TransferShrinkageToPlastics();
		}

		private void PullFromWeb()
		{
			HtmlDocument doc;
			var web = new HtmlWeb();

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			web.AutoDetectEncoding = false;
			web.OverrideEncoding = Encoding.UTF8;

			doc = web.Load(Url);
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
				if (currentNode.Name != "h2")
					continue;

				var nextNodes = new List<HtmlNode>();
				for (int j = 1; i + j < filteredList.Count && filteredList[i + j].Name == "table"; j++)
				{
					nextNodes.Add(filteredList[i + j]);
				}

				switch (currentNode.InnerText.ToLower())
				{
					case ("customer"):
						ProcessCustomer(nextNodes);
						break;
					case ("notes"):
						ProcessProjectNotes(nextNodes[0]);
						break;
					case ("material"):
						ProcessMaterial(nextNodes[0]);
						break;
					case ("operations"):
						ProcessOperations(nextNodes[0]);
						break;
					case ("plastics"):
						Plastics = GetPlasticsFromNode(nextNodes[0]);
						break;
					default:
						break;
				}
			}
		}

		private void ProcessCustomer(List<HtmlNode> nextNodes)
		{
			var customerNode = nextNodes[0].SelectNodes(".//td")[0];
			Customer = WebUtility.HtmlDecode(customerNode?.InnerText) ?? "";
			var descriptionNode = nextNodes[1]?.SelectNodes(".//td")[0];
			Description = WebUtility.HtmlDecode(descriptionNode?.InnerText) ?? "";
		}

		private ArticleType ResolveType()
		{
			if (Regex.IsMatch(Name, @"^\d{6}[VP](?:\d|-\d)$"))
				IsModOrVariant = true;

			if (Regex.IsMatch(Name, @"^\d{6}V\d?$"))
				return ArticleType.Tool;
			else if (Regex.IsMatch(Name, @"^\d{6}P(?:-\d)?$"))
				return ArticleType.Plastic;
			else
				throw new ArgumentException($"Couldn't find type for article {Name}");
		}
		private string GenerateUrl()
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
		// Gets Machine
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
		// Gets Material & Shrinkage
		private void ProcessMaterial(HtmlNode rootNode)
		{
			Regex regex = new Regex(@"^\d+(?:-\d)? +- +(?<Material>.+\b\)?)(?: +(?<Shrinkage>(?:\b\d(?:[,.]\d+)?|[Xx])%))?(?:\s+)?$");

			// TODO: Improve Readability
			foreach (var rootChild in rootNode.ChildNodes[2].ChildNodes)
			{
				var childNodes = rootChild.ChildNodes;
				if (!childNodes.Any() || childNodes[0].InnerText.ToLower() != "plastic")
					continue;

				var rawData = childNodes[2]?.InnerText;
				if (rawData == null)
					return;

				var decodedData = WebUtility.HtmlDecode(rawData);
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
					if (Shrinkage is not null && 
						!Shrinkage.ContainsKey(Name))
						Shrinkage.Add(Name, shrinkageCapture.Value);

				break;
			}
		}
		private void ProcessProjectNotes(HtmlNode rootNode)
		{
			var rawData = rootNode.SelectNodes(".//td")[1].InnerText;
			var decodedData = WebUtility.HtmlDecode(rawData);
			var regExResults = Regex.Matches(decodedData, @"(?:\d{6} (?<CAD>\w+) \// (?=(?:[Kk]rymp|\d{6}P))|(?<Plastic>(\d{6}P)) (?<Shrinkage>[Kk]rymp\s*\d(?:[,.]\d+)?%)|(?<Shrinkage>[Kk]rymp\s*\d(?:[,.]\d+)?%))");

			foreach (Match result in regExResults)
			{
				if (!result.Success ||
					result.Groups.Count < 1)
					return;

				
				var cadOperatorCapture = result.Groups["CAD"];
				if (cadOperatorCapture.Success &&
					!cadOperatorCapture.Value.Equals("Andreas"))
					CAD = cadOperatorCapture.Value;

				var PartNameCapture = result.Groups["Plastic"];
				var PartName = PartNameCapture.Value;

				if (!PartNameCapture.Success)
					PartName = Name;

				var shrinkageCapture = result.Groups["Shrinkage"];
				if (shrinkageCapture.Success)
					if (Shrinkage is not null && 
						!Shrinkage.ContainsKey(PartName))
						Shrinkage.Add(PartName, shrinkageCapture.Value.ToLower().Replace("krymp ", ""));
			}
		}
		private List<ArticleInfo> GetPlasticsFromNode(HtmlNode rootNode)
		{
			var output = new List<ArticleInfo>();

			var tdNodes = rootNode.SelectNodes(".//td");
			for (int i = 0; i < tdNodes?.Count; i++)
			{
				if (i % 3 == 0 && IsPlasticsNode(tdNodes[i]))
					output.Add(new ArticleInfo(tdNodes[i].InnerText));
			}

			return output;
		}
		private void TransferShrinkageToPlastics()
		{
			if (Plastics is null ||
				!Plastics.Any())
				return;

			foreach (var plastic in Plastics)
			{
				if (plastic is null)
					continue;
				if (!Shrinkage.ContainsKey(plastic.Name))
					continue;

				if (plastic.Shrinkage.ContainsKey(plastic.Name))
					plastic.Shrinkage[plastic.Name] = Shrinkage[plastic.Name];
				else
					plastic.Shrinkage.Add(plastic.Name, Shrinkage[plastic.Name]);
			}
		}

		private bool IsPlasticsNode(HtmlNode node)
		{
			bool output = Regex.IsMatch(node.InnerText, @"^\d{6}P(?:-\d)?$");
			return output;
		}
		private bool IsNameValid(string name)
		{
			if (name == null ||
				string.IsNullOrWhiteSpace(name) ||
				!Regex.IsMatch(name, @"^\d{6}[VvPp](?:\d|-\d)?$"))
			{
				return false;
			}
			return true;
		}
	}
}