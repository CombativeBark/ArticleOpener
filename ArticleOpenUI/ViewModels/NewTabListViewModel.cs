﻿using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ArticleOpenUI.Events;
using ArticleOpenUI.Models;

namespace ArticleOpenUI.ViewModels
{
	class NewTabListViewModel : Screen, IListTabItem
	{
		private readonly IEventAggregator m_EventAggregator;
		public ObservableCollection<ArticleModel> Articles { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public TabItemType Type { get => TabItemType.NewTab; }

		public void AddArticle(ArticleModel article)
		{
			throw new NotImplementedException();
		}
		public NewTabListViewModel(IEventAggregator eventAggregator)
		{
			m_EventAggregator = eventAggregator;
			DisplayName = "+";
		}
		protected override Task OnActivateAsync(CancellationToken cancellationToken)
		{
			m_EventAggregator.PublishOnUIThreadAsync(new NewTabEvent());
			return base.OnActivateAsync(cancellationToken);
		}

	}
}
