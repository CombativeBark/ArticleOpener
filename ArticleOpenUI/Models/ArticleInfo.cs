﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ArticleOpenUI.Models
{
	public class ArticleInfo
	{
		public string Name { get; set; } = "";
		public bool IsModOrVariant { get; set; } = false;
		public List<ArticleInfo> Plastics { get; set; } = new List<ArticleInfo>();
		public string CAD { get; set; } = "Unknown";
		public string Customer { get; set; } = "Unknown";
		public string Description { get; set; } = "Unknown";
		public string Material { get; set; } = "Unknown";
		public Dictionary<string, string> Shrinkage { get; set; } = new Dictionary<string, string>();
		public string Machine { get; set; } = "Unknown";

		public ArticleType Type
		{
			get
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
		}
		public string Url 
		{
			get
			{
				return Type switch
				{
					ArticleType.Tool => $@"http://server1:85/tool/{Name}",
					ArticleType.Plastic => $@"http://server1:85/plastic/{Name}",
					_ => throw new ArgumentException($"Article type of {Name} is not support"),
				};
			}
		}

		public ArticleInfo(string name)
		{
			if (IsNameValid(name))
				Name = name;
			else
				throw new ArgumentException($"{name} is not a valid article.");

			ScrapeInfoPage();
			TransferShrinkageToPlastics();
		}

		private void ScrapeInfoPage()
		{
			HtmlDocument document;
			var web = new HtmlWeb();

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			web.AutoDetectEncoding = false;
			web.OverrideEncoding = Encoding.UTF8;

			document = web.Load(Url);
			if (document == null || web.StatusCode != System.Net.HttpStatusCode.OK)
				throw new HtmlWebException($"Couldn't load web page for {Name}.");

			var rootNode = document.DocumentNode
				.SelectSingleNode("//body/div[@class='page']/div[@class='main']/div[@class='content px-4']");
			if (rootNode == null)
				throw new NodeNotFoundException($"Couldn't find parse web page for {Name}");

			var filteredNodeList = rootNode.ChildNodes
				.Where(x => x.Name != "#text")
				.ToList();

			for (int i = 1; i < filteredNodeList.Count; i++)
			{
				var currentNode = filteredNodeList[i];
				if (currentNode.Name != "h2")
					continue;

				var nextNodes = new List<HtmlNode>();
				for (int j = 1; i + j < filteredNodeList.Count && filteredNodeList[i + j].Name == "table"; j++)
					nextNodes.Add(filteredNodeList[i + j]);

				switch (currentNode.InnerText.ToLower())
				{
					case ("customer"):
						ProcessCustomerTable(nextNodes);
						break;
					case ("notes"):
						ProcessNotesTable(nextNodes[0]);
						break;
					case ("material"):
						ProcessMaterialTable(nextNodes[0]);
						break;
					case ("operations"):
						ProcessOperationsTable(nextNodes[0]);
						break;
					case ("plastics"):
						Plastics = ProcessPlastics(nextNodes[0]);
						break;
					default:
						break;
				}
			}
		}
		private void ProcessCustomerTable(List<HtmlNode> customerNodes)
		{
			var customerProperty = customerNodes[0].SelectNodes(".//td")[0];
			Customer = WebUtility.HtmlDecode(customerProperty?.InnerText) ?? "";

			var descriptionProperty = customerNodes[1]?.SelectNodes(".//td")[0];
			Description = WebUtility.HtmlDecode(descriptionProperty?.InnerText) ?? "";
		}
		// Gets Machine
		private void ProcessOperationsTable(HtmlNode operationsTable)
		{
			var regex = new Regex(@"^21[1-9]0$", RegexOptions.Compiled);

			foreach (var child in operationsTable.ChildNodes[2].ChildNodes)
			{
				if (!regex.IsMatch(child.ChildNodes[0].InnerText))
					continue;

				Machine = WebUtility.HtmlDecode(child.ChildNodes[2].InnerText);
				break;
			}
		}
		// Gets Material & Shrinkage
		private void ProcessMaterialTable(HtmlNode materialTable)
		{
			var regexMaterial = new Regex(@"^\d+(?:-\d)? +- +(?<Material>.+\b\)?)(?: +(?<Shrinkage>(?:\b\d(?:[,.]\d+)?|[Xx])(?:-\d(?:[,.]\d+)?)?%))?(?:\s+)?$");

			// TODO: Improve Readability
			foreach (var tableItem in materialTable.ChildNodes[2].ChildNodes)
			{
				var tableItemProperties = tableItem.ChildNodes;
				if (!tableItemProperties.Any() || tableItemProperties[0].InnerText.ToLower() != "plastic")
					continue;

				var plasticPropertyValue = tableItemProperties[2]?.InnerText;
				if (plasticPropertyValue == null)
					return;

				var decodedPlasticValue = WebUtility.HtmlDecode(plasticPropertyValue);
				var regexMaterialResult = regexMaterial.Match(decodedPlasticValue);
				if (!regexMaterialResult.Success)
					return;

				var materialCapture = regexMaterialResult.Groups["Material"];
				if (materialCapture.Success)
					Material = materialCapture.Value;

				if (regexMaterialResult.Groups.Count < 2)
					return;

				var shrinkageCapture = regexMaterialResult.Groups["Shrinkage"];
				if (shrinkageCapture.Success)
					if (Shrinkage is not null && 
						!Shrinkage.ContainsKey(Name))
						Shrinkage.Add(Name, shrinkageCapture.Value);

				break;
			}
		}
		private void ProcessNotesTable(HtmlNode notesTable)
		{
			var regexInfoPattern = @"(?:\d{6} (?<CAD>\w+) \// (?=(?:[Kk]rymp|\d{6}P))|(?<Plastic>(\d{6}P)) (?<Shrinkage>[Kk]rymp\s*\d(?:[,.]\d+)?%)|(?<Shrinkage>[Kk]rymp\s*\d(?:[,.]\d+)?%))";

			var rawInfoData = notesTable.SelectNodes(".//td")[1].InnerText;
			var decodedInfoData= WebUtility.HtmlDecode(rawInfoData);
			var regexInfoMatches = Regex.Matches(decodedInfoData, regexInfoPattern);

			foreach (Match infoMatch in regexInfoMatches.Cast<Match>())
			{
				if (!infoMatch.Success ||
					infoMatch.Groups.Count < 1)
					return;

				
				var cadOperatorCapture = infoMatch.Groups["CAD"];
				if (cadOperatorCapture.Success && CAD == "Unknown") 
					CAD = cadOperatorCapture.Value;

				var partNameCapture = infoMatch.Groups["Plastic"];
				var partName = partNameCapture.Value;
				if (!partNameCapture.Success)
					partName = Name;

				var shrinkageCapture = infoMatch.Groups["Shrinkage"];
				if (shrinkageCapture.Success)
					if (Shrinkage is not null && 
						!Shrinkage.ContainsKey(partName))
						Shrinkage.Add(partName, shrinkageCapture.Value.ToLower().Replace("krymp ", ""));
			}
		}
		private List<ArticleInfo> ProcessPlastics(HtmlNode plasticsNode)
		{
			var result = new List<ArticleInfo>();

			var tableItems = plasticsNode.SelectNodes(".//td");
			for (int i = 0; i < tableItems?.Count; i++)
			{
				if (i % 3 == 0 && IsPlasticsNode(tableItems[i]))
					result.Add(new ArticleInfo(tableItems[i].InnerText));
			}

			return result;
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