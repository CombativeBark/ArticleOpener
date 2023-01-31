using System;
using System.Collections.Generic;

namespace ArticleOpenUI.Models
{
	public static class ArticleFactory
	{
		public static ArticleBase CreateArticle(string name)
		{
			var articleName = name.ToUpper();
			var info = new ArticleInfo(articleName);

			if (info == null)
				throw new ArgumentException($"Error: Can't process info for article {articleName}");

			switch (info.Type)
			{
				case ArticleType.Tool:
					var toolArticle = new ToolArticle(info);
					return toolArticle;
				case ArticleType.Plastic:
					var plasticArticle = new PlasticArticle(info);
					return plasticArticle;
				default:
					throw new Exception($"Error: {articleName} is not a supported article type");
			}
		}
	}
}