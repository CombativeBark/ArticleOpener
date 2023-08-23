using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ArticleOpenUI.ViewModels
{
	class ArticleViewModel : Conductor<ArticleListViewModel>.Collection.OneActive
	{
		private IEventAggregator m_EventAggregator;
		private IWindowManager m_WindowManager;
		private ArticleListViewModel m_SelectedArticleList;
		private string m_Input;
		private bool m_OpenToolsFilter;
		private bool m_OpenPlasticsFilter;
		private bool m_OpenInfoFilter;
		private bool m_OpenFoldersFilter;

		public ArticleListViewModel SelectedArticleList
		{
			get => m_SelectedArticleList;
			set
			{
				m_SelectedArticleList = value;
				NotifyOfPropertyChange(() => SelectedArticleList);
			}
		}
		public string Input
		{
			get { return m_Input; }
			set
			{
				m_Input = value;
				NotifyOfPropertyChange(() => Input);
			}
		}
		public bool OpenToolsFilter
		{
			get
			{
				return m_OpenToolsFilter;
			}
			set
			{
				m_OpenToolsFilter = value;
				NotifyOfPropertyChange(() => OpenToolsFilter);
			}
		}
		public bool OpenPlasticsFilter
		{
			get
			{
				return m_OpenPlasticsFilter;
			}
			set
			{
				m_OpenPlasticsFilter = value;
				NotifyOfPropertyChange(() => OpenPlasticsFilter);
			}
		}
		public bool OpenInfoFilter
		{
			get
			{
				return m_OpenInfoFilter;
			}
			set
			{
				m_OpenInfoFilter = value;
				NotifyOfPropertyChange(() => OpenInfoFilter);
			}
		}
		public bool OpenFoldersFilter
		{
			get
			{
				return m_OpenFoldersFilter;
			}
			set
			{
				m_OpenFoldersFilter = value;
				NotifyOfPropertyChange(() => OpenFoldersFilter);
			}
		}

		public ArticleViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
		{
			m_EventAggregator = eventAggregator;
			m_WindowManager = windowManager;

			m_Input = "";
			OpenToolsFilter = true;
			OpenPlasticsFilter = true;
			OpenInfoFilter = true;
			OpenFoldersFilter = true;
			Items.Add(new ArticleListViewModel(windowManager, eventAggregator));
			SelectedArticleList = Items.First();

#if DEBUG
			m_Input = "302981V";
#endif
		}
		public void TextBoxEvent(ActionExecutionContext context)
		{
			var keyArgs = context.EventArgs as KeyEventArgs;

			if (keyArgs == null || string.IsNullOrWhiteSpace(Input))
				return;

			switch (keyArgs.Key)
			{
				case Key.Enter:
					SearchArticle();
					break;
				case Key.Escape:
					ClearQueue();
					break;
				default:
					break;
			}
		}
		public void SearchArticle()
		{
			if (Input == null || string.IsNullOrEmpty(Input))
				return;

			foreach (var articleNumber in SplitString(Input))
			{
				try
				{
					var article = ArticleFactory.CreateArticle(articleNumber);

					AddToQueue(article);
					if (article.Children != null && article.Children.Any())
					{
						article.Children.ForEach(x =>
						{
							try
							{
								var plasticArticle = ArticleFactory.CreateArticle(x);
								AddToQueue(plasticArticle);
							}
							catch (Exception e)
							{
								MessageBox.Show("Error: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							}
						});
					}
				}
				catch (Exception e)
				{
#if (DEBUG)
					MessageBox.Show($"Error: {e.Message}\n\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#else
					MessageBox.Show("Error: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

				}
			}
		}
		public void OpenArticlesInQueue()
		{
			if (OpenArticles != null &&
				SelectedArticleList.Articles.Count > 0)
			{
				try
				{
					OpenArticles();
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			else
			{
				MessageBox.Show("No articles to open");
			}
		}
		// Double-click to clear input
		public void ClearQueue()
		{
			SelectedArticleList.Articles.Clear();
		}
		private void OpenArticles()
		{
			foreach (var article in SelectedArticleList.Articles)
			{

				if ((!OpenToolsFilter && article.Type == ArticleType.Tool) ||
				(!OpenPlasticsFilter && article.Type == ArticleType.Plastic))
					return;

				if (OpenFoldersFilter)
				{
					article.OpenFolder();
				}

				if (OpenInfoFilter)
				{
					article.OpenInfo();
					Thread.Sleep(100);
				}

			};
		}
		private void AddToQueue(ArticleModel article)
		{
			if (!IsInQueue(article.Name))
			{
				SelectedArticleList.Articles.Add(article);
			}
		}
		private bool IsInQueue(string inputArticle)
		{
			foreach (var article in SelectedArticleList.Articles)
			{
				if (article.Name.Equals(inputArticle))
					return true;
			}
			return false;
		}
		private List<string> SplitString(string input)
		{
			List<string> result = new();

			var splitString = input.Split(new char[] { ' ', '.', ':', ',', ';', '-', '_' });
			for (int i = 0; i < splitString.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(splitString[i]))
				{
					result.Add(splitString[i]);
				}
			}
			return result;
		}
		public void CloseTab(object sender)
		{
			var context = sender as ArticleListViewModel;
			if (context == null)
				return;

			Items.Remove(context);
			if (Items.Count == 0)
				CreateNewTab();
		}
		public void CreateNewTab()
		{
			var newTab = new ArticleListViewModel(m_WindowManager, m_EventAggregator);
			Items.Add(newTab);
			SelectedArticleList = newTab;
		}
	}
}
