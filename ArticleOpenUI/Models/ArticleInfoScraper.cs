using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ArticleOpenUI.Models
{
	public static class ArticleInfoScraper
	{
		private static HtmlDocument m_WebDocument { get; set; }

		public static ArticleInfoModel GetArticleInfo(string inputName)
		{
			try
			{
				var output = new ArticleInfoModel(inputName);
				m_WebDocument = GetWebDocument(output.Url);
				return output;
			}
			catch (Exception ex)
			{
				throw;
			}

		}

		private static HtmlDocument GetWebDocument(string name)
		{
			return new HtmlDocument();
		}
		private static void scrape(ref ArticleInfoModel info)
		{
			HtmlDocument document;
			var web = new HtmlWeb();

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			web.AutoDetectEncoding = false;
			web.OverrideEncoding = Encoding.UTF8;

			document = web.Load(info.Url);
			if (document == null || web.StatusCode != System.Net.HttpStatusCode.OK)
				throw new HtmlWebException($"Couldn't load web page for {info.Name}.");

			var rootNode = document.DocumentNode
				.SelectSingleNode("//body/div[@class='page']/div[@class='main']/div[@class='content px-4']");
			if (rootNode == null)
				throw new NodeNotFoundException($"Couldn't find parse web page for {info.Name}");

			var filteredNodeList = rootNode.ChildNodes
				.Where(node => node.Name != "#text")
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
						ProcessCustomerTable(ref info, nextNodes);
						break;
					case ("notes"):
						ProcessNotesTable(ref info, nextNodes[0]);
						break;
					case ("material"):
						ProcessMaterialTable(ref info, nextNodes[0]);
						break;
					case ("operations"):
						ProcessOperationsTable(ref info, nextNodes[0]);
						break;
					case ("plastics"):
						info.Plastics = ProcessPlastics(nextNodes[0]);
						break;
					default:
						break;
				}
			}
		}
		private static void ProcessCustomerTable(ref ArticleInfoModel info, List<HtmlNode> customerNodes)
		{
			var customerProperty = customerNodes[0].SelectNodes(".//td")[0];
			info.Customer = WebUtility.HtmlDecode(customerProperty?.InnerText) ?? "";

			var descriptionProperty = customerNodes[1]?.SelectNodes(".//td")[0];
			info.Description = WebUtility.HtmlDecode(descriptionProperty?.InnerText) ?? "";
		}
		// Gets Machine
		private static void ProcessOperationsTable(ref ArticleInfoModel info, HtmlNode operationsTable)
		{
			var regex = new Regex(@"^21[1-9]0$", RegexOptions.Compiled);

			foreach (var child in operationsTable.ChildNodes[2].ChildNodes)
			{
				if (!regex.IsMatch(child.ChildNodes[0].InnerText))
					continue;

				info.Machine = WebUtility.HtmlDecode(child.ChildNodes[2].InnerText);
				break;
			}
		}
		// Gets Material & Shrinkage
		private static void ProcessMaterialTable(ref ArticleInfoModel info, HtmlNode materialTable)
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
					info.Material = materialCapture.Value;

				if (regexMaterialResult.Groups.Count < 2)
					return;

				var shrinkageCapture = regexMaterialResult.Groups["Shrinkage"];
				if (shrinkageCapture.Success)
					if (info.Shrinkage is not null && 
						!info.Shrinkage.ContainsKey(info.Name))
						info.Shrinkage.Add(info.Name, shrinkageCapture.Value);

				break;
			}
		}
		private static void ProcessNotesTable(ref ArticleInfoModel info, HtmlNode notesTable)
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
				if (cadOperatorCapture.Success && info.CAD == "Unknown") 
					info.CAD = cadOperatorCapture.Value;

				var partNameCapture = infoMatch.Groups["Plastic"];
				var partName = partNameCapture.Value;
				if (!partNameCapture.Success)
					partName = info.Name;

				var shrinkageCapture = infoMatch.Groups["Shrinkage"];
				if (shrinkageCapture.Success)
					if (info.Shrinkage is not null && 
						!info.Shrinkage.ContainsKey(partName))
						info.Shrinkage.Add(partName, shrinkageCapture.Value.ToLower().Replace("krymp ", ""));
			}
		}
		private static List<string> ProcessPlastics(HtmlNode plasticsNode)
		{
			var result = new List<string>();

			var tableItems = plasticsNode.SelectNodes(".//td");
			for (int i = 0; i < tableItems?.Count; i++)
			{
				if (i % 3 == 0 && IsPlasticsNode(tableItems[i]))
					result.Add(tableItems[i].InnerText);
			}

			return result;
		}
		/*
		private void TransferShrinkageToPlastics(ref ArticleInfoModel info)
		{
			if (info.Plastics is null ||
				!info.Plastics.Any())
				return;

			foreach (var plastic in info.Plastics)
			{
				if (plastic is null)
					continue;
				if (!info.Shrinkage.ContainsKey(plastic.Name))
					continue;

				if (plastic.Shrinkage.ContainsKey(plastic.Name))
					plastic.Shrinkage[plastic.Name] = info.Shrinkage[plastic.Name];
				else
					plastic.Shrinkage.Add(plastic.Name, info.Shrinkage[plastic.Name]);
			}
		}
		*/
		private static bool IsPlasticsNode(HtmlNode node)
		{
			bool output = Regex.IsMatch(node.InnerText, @"^\d{6}P(?:-\d)?$");
			return output;
		}
		private static bool IsValidArticleID(string name)
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