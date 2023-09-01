using System.Windows;
using System.Windows.Controls;
using ArticleOpenUI.Models;

namespace ArticleOpenUI.Helpers
{
	public class ListTemplateSelector : DataTemplateSelector
	{
		public DataTemplate ArticleListTemplate { get; set; }
		public DataTemplate NewTabTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var selectedTemplate = ArticleListTemplate;

			if (item is not IListTabItem listItem)
				return selectedTemplate;

			switch (listItem.Type)
			{
				case TabItemType.ArticleList:
					selectedTemplate = ArticleListTemplate;
					break;
				case TabItemType.NewTab:
					selectedTemplate = NewTabTemplate;
					break;
			}

			return selectedTemplate;
		}
	}
}
