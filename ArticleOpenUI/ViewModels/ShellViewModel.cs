using Caliburn.Micro;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleOpenUI.ViewModels
{
	class ShellViewModel : Conductor<object>
	{
		private readonly IWindowManager m_WindowManager;
		private readonly ArticleViewModel m_ArticleViewModel;
		public ShellViewModel(ArticleViewModel articleViewModel, IWindowManager windowManager)
		{
			m_WindowManager = windowManager;
			m_ArticleViewModel = articleViewModel;
			ActivateItemAsync(m_ArticleViewModel);
		}

		public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
		{
			bool hasArticlesOpen = false;
			bool cancelConfirmation = true;

			foreach (var articleList in m_ArticleViewModel.Items)
				if (articleList is not NewTabListViewModel && articleList.Articles.Any())
					hasArticlesOpen = true;

			if (hasArticlesOpen)
			{
				var result = MessageBox.Show("You still have articles open.\nAre you sure you want to close?", "Close Window", MessageBoxButton.YesNo, MessageBoxImage.Warning);
				cancelConfirmation = result == MessageBoxResult.Yes;
			}
			if (cancelConfirmation)
				return base.CanCloseAsync(cancellationToken);
			else
				return new Task<bool>(() => false);

		}
	}
}
