using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace ArticleOpenUI.Models
{
	public interface IListTabItem : IScreen
	{
		public TabItemType Type { get; }
		public ObservableCollection<ArticleModel> Articles { get; }
		public void AddArticle(ArticleModel article);
	}
}