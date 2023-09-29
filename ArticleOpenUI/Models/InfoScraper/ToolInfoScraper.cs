using ArticleOpenUI.Models.Article;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models.InfoScraper
{
	public class ToolInfoScraper : InfoScraperBase
	{
		public ToolInfoScraper(HtmlWeb web) : base(web)
		{
		}

		public void GatherInfo(ToolModel article)
		{
			base.GatherInfo(article);

			article.CAD = GetCADOperator(Document);
		}

		private string GetCADOperator(HtmlDocument document)
		{
			throw new NotImplementedException();
		}

		public override string GetShrinkage(HtmlDocument document)
		{
			throw new NotImplementedException();
		}

		public override string ResolvePath(string name, bool isSpecial)
		{
			const string rootPath = @"\\server1\ArtikelFiler\ArticleFiles\";
			string basePath = Path.Join(rootPath, name);

			if (isSpecial)
				return basePath[0..^1];

			return Path.Join(basePath, name);
		}

		public override string ResolveUrl(string name)
		{
			return $"http://server1:85/tool/{name}";
		}
	}
}
