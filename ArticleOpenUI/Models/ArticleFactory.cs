using System;

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
		public static ArticleModel CreateArticle(ArticleInfo info)
		{
			if (info == null)
				throw new ArgumentNullException(nameof(info));

			return new ArticleModel(info);
		}
	}
}