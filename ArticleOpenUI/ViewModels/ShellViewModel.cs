using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
