using ArticleOpenUI.Models.Article;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models.InfoScraper
{
	public abstract class InfoScraperBase	
	{
		private Regex reSpecial { get; } = new Regex(@"\d{6}[VP].+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private HtmlWeb m_Web { get; set; }

		public HtmlDocument Document { get; set; }

		public InfoScraperBase(HtmlWeb web)
		{
			m_Web = web;
		}

		public virtual void GatherInfo(IArticle article)
		{
			if (article == null)
				throw new ArgumentNullException((nameof(article)));

			ParseInfoFromName(article);

			Document = GetHTMLDocument(article.Url);
			if (Document is null)
				throw new NullReferenceException(nameof(Document));

			ParseInfoFromDocument(article, Document);
		}

		public void ParseInfoFromName(IArticle article)
		{
			article.IsSpecial = ResolveSpecial(article.Name);
			article.Path = ResolvePath(article.Name, article.IsSpecial);
			article.Url = ResolveUrl(article.Name);
		}
		public abstract string ResolvePath(string name, bool isSpecial);
		public abstract string ResolveUrl(string name);

		public void ParseInfoFromDocument(IArticle article, HtmlDocument document)
		{
			var tableNodes = document.DocumentNode.SelectNodes("/html/body/div[1]/div[2]/div[2]");

			for (int i = 0; i < tableNodes.Count; i++)
			{
				var currentTableNode = tableNodes[i];
				switch (currentTableNode.InnerText.ToLower())
				{
					case "customer":
				article.Customer = GetCustomer(tableNodes[i + 1]);
						break;
					case "operations":
				article.Machine = GetMachine(tableNodes[i+1]);
						break;
					case "notes":
				article.Shrinkage = GetShrinkage(tableNodes[i + 1]);
						break;
					default:
						break;
				}

			}
		}

		public abstract string GetShrinkage(HtmlNode shrinkageNode);
		private string GetMachine(HtmlNode machineNode)
		{
			var machine = GetTableEntries(machineNode)[0].Where(x => x.Key.StartsWith("21"));
			return machine.First().Value;
		}
		private string GetCustomer(HtmlNode customerTableNode)
		{
			var customer = GetTableEntries(customerTableNode)["Name"];
			return customer;
		}

		private HtmlDocument GetHTMLDocument(string url)
		{
			HtmlDocument output = m_Web.Load(url);

			if (output == null)
				throw new NullReferenceException(nameof(output));

			return output;
		}

		private bool ResolveSpecial(string name)
		{
			if (reSpecial.IsMatch(name))
				return true;

			return false;
		}

		private List<Dictionary<string, string>> GetTableEntries(HtmlNode tableNode)
		{
			var output = new List<Dictionary<string, string>>();
			var tableHeaders = tableNode.SelectNodes("/thead/tr/th");
			var tableRows = tableNode.SelectNodes("/tbody/tr");

			string[] keys = new string[tableHeaders.Count];
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i] = WebUtility.HtmlDecode(tableHeaders[i].InnerText);
			}

			for (int i = 0; i < tableRows.Count; i++)
			{
				var row = tableRows[i];
				var rowValues = new Dictionary<string, string>();
				for (int j = 0; j < row.ChildNodes.Count; j++)
				{
					// keys[0] = Pgrp, [1] = Description, etc...
					var key = keys[j];
					// row.ChildNodes[0] = 1010, [1] = Projekt, etc...
					var value = WebUtility.HtmlDecode(row.ChildNodes[j].InnerText);

					rowValues.Add(key, value);
				}
				output.Add(rowValues);
			}

			return output;
		}
	}
}