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
		public MouldSelectViewModel(IEventAggregator eventAggregator)
		{
			MouldList = new ObservableCollection<string>();
			m_EventAggregator = eventAggregator;
		}

		public void PopulateList(IEnumerable<string> files)
		{
			files.ToList()
				.ForEach(file => MouldList.Add(file));
			MouldList.Concat(files);
		}
	}
}
