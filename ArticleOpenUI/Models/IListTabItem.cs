using System.Collections.ObjectModel;
using ArticleOpenUI.Models.Article;
using Caliburn.Micro;

namespace ArticleOpenUI.Models
{
    public enum TabItemType
    {
        ArticleList,
        NewTab
    }

    public interface IListTabItem : IScreen
	{
		public TabItemType Type { get; }
		public ObservableCollection<IArticle> Articles { get; }
		public void AddArticle(IArticle article);
	}
}