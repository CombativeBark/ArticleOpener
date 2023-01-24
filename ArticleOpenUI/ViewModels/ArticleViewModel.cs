using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ArticleOpenUI.ViewModels
{

	class ArticleViewModel : Screen
	{
		private List<ArticleBase> m_ArticleQueue;
		private string m_Input;
		private bool m_OpenToolsFilter;
		private bool m_OpenPlasticsFilter;
		private bool m_OpenInfoFilter;
		private bool m_OpenFoldersFilter;

		public ObservableCollection<ArticleBase> ArticleList { get; private set; }
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

		public ArticleViewModel()
		{
			m_ArticleQueue = new();
			m_Input = "";

			OpenToolsFilter = true;
			OpenPlasticsFilter = true;
			OpenInfoFilter = true;
			OpenFoldersFilter = true;
			ArticleList = new();
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
					if (article.Children != null && article.Type == ArticleType.Tool)
					{
						article.Children.ForEach(x => ArticleFactory.CreateArticle(x));
					}
				}
				catch (Exception e)
				{
					#if (DEBUG)
					MessageBox.Show($"Error: {e.Message}\n\n{e.StackTrace}" , "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					#else
					MessageBox.Show("Error: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					#endif

				}
			}
		}
		public void OpenArticlesInQueue()
		{
			if (m_ArticleQueue != null &&
				m_ArticleQueue.Count > 0)
            {
				try
				{
					OpenArticles(ArticleOpenMode.All, ArticleOpenFilter.All);
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
		public void ClearQueue()
		{
			m_ArticleQueue.Clear();
			ArticleList.Clear();
		}
		public void OpenFolder(object? context)
		{
			var article = context as ArticleBase;
			if (article == null) return;

			article.OpenFolder();
		}
		public void OpenInfo(object? context)
		{
			var article = context as ArticleBase;
			if (article == null) return;

			article.OpenInfo();
		}
		public void RemoveFromQueue(object? context)
		{
			var item = context as ArticleBase;

			if (item == null)
				return;

			foreach (var article in m_ArticleQueue)
			{
				if (article.Name == item.Name)
				{
					m_ArticleQueue.Remove(article);
					ArticleList.Remove(article);
					return;
				}
			}
		}
        private void OpenArticles(ArticleOpenMode openMode, ArticleOpenFilter openFilter)
		{
			m_ArticleQueue.ForEach(article =>
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

			});
		}
		private void AddToQueue(ArticleBase article)
		{
			if (!IsInQueue(article.Name))
			{
				m_ArticleQueue.Add(article);
				ArticleList.Add(article);
			}
		}
		private bool IsInQueue(string inputArticle)
		{
			foreach (var article in m_ArticleQueue)
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
    }
}
