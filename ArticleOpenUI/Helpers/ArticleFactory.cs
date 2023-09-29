using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ArticleOpenUI.Models;
using ArticleOpenUI.Models.Article;
using ArticleOpenUI.Models.InfoScraper;

namespace ArticleOpenUI.Helpers
{
    public static class ArticleFactory
	{
		public static readonly HtmlAgilityPack.HtmlWeb Web = new();
		public static readonly ToolInfoScraper toolInfoScraper = new ToolInfoScraper(Web);
		//public static readonly PlasticInfoScraper plasticInfoScraper = new PlasticInfoScraper();

		public static List<IArticle> CreateArticle(params string[] articles)
		{
			List<IArticle> output = new List<IArticle>();

			foreach (string article in articles)
				output.Add(CreateArticle(article));

			return output;
		}

		public static IArticle CreateArticle(string name)
		{
			if (name.Trim().ToUpper() is not string normalizedName)
				throw new ArgumentException(nameof(name));
			
			IArticle article;

			switch (normalizedName[6])
			{
				case 'V':
					article = new ToolModel();
					article.Name = normalizedName;
					toolInfoScraper.GatherInfo(article);
					break;
					/*
				case 'P':
					article = new PlasticModel();
					break;
					*/
				default:
					throw new ArgumentException($"Article name {name} doesn't contain type identifier");
			}

			return article;
		}
	}
}