using System;
using System.Collections.Generic;

namespace ArticleOpenUI.Models
{
	public static class ArticleFactory
	{
		public static ArticleModel CreateArticle(string name)
		{
			var articleName = name.ToUpper();
			var info = new ArticleInfo(articleName);

			if (info == null)
				throw new ArgumentException($"Error: Can't process info for article {articleName}");

			return new ArticleModel(info);
		}
	}
}