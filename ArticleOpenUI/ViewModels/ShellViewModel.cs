using Caliburn.Micro;
using System.Threading;
using System.Threading.Tasks;

namespace ArticleOpenUI.ViewModels
{
	class ShellViewModel : Conductor<object>
	{
		private readonly ArticleViewModel m_ArticleViewModel;
		public ShellViewModel(ArticleViewModel articleViewModel)
		{
			m_ArticleViewModel = articleViewModel;
			ActivateItemAsync(m_ArticleViewModel);
		}
	}
}
