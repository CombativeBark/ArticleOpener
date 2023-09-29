using System.Collections.Generic;

namespace ArticleOpenUI.Models.Article
{
	public interface IArticle
	{
		string Name { get; set; }
		bool IsSpecial { get; set; }
		string Url { get; set; }
		string Path { get; set; }
		string Customer { get; set; }
		string Machine { get; set; }
		string Shrinkage { get; set; }

		void OpenFolder();
		void OpenInfo();
	}
}