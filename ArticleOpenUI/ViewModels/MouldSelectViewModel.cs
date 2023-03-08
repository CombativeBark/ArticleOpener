using Caliburn.Micro;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ArticleOpenUI.ViewModels
{
	class MouldSelectViewModel : Screen
	{
		private IEventAggregator m_EventAggregator;

		public ObservableCollection<string> MouldList;
		public MouldSelectViewModel(IEnumerable<string> mldList, IEventAggregator eventAggregator)
		{
			MouldList = new ObservableCollection<string>(mldList);
			m_EventAggregator = eventAggregator;
		}
	}
}
