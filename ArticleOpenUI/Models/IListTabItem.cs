using Caliburn.Micro;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ArticleOpenUI.Models
{
	public interface IListTabItem : IScreen
	{
		public TabItemType Type { get; }
		public ObservableCollection<ArticleModel> Articles { get; }
		public void AddArticle(ArticleModel article);
	}
}