using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ArticleOpenUI.ViewModels
{
	class ArticleViewModel : Screen
	{
		private List<ArticleModel> m_ArticleQueue;
		private string m_Input;
		private bool m_OpenToolsFilter;
		private bool m_OpenPlasticsFilter;
		private bool m_OpenInfoFilter;
		private bool m_OpenFoldersFilter;
		private IEventAggregator m_EventAggregator;
		private IWindowManager m_WindowManager;

		public ObservableCollection<ArticleModel> ArticleList { get; private set; }
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
			m_ArticleQueue = new List<ArticleModel>();
			m_Input = "";
			m_EventAggregator = eventAggregator;
			m_WindowManager = windowManager;

			OpenToolsFilter = true;
			OpenPlasticsFilter = true;
			OpenInfoFilter = true;
			OpenFoldersFilter = true;
			ArticleList = new ObservableCollection<ArticleModel>();

#if (DEBUG)
			m_Input = "302981V";

			var testInfo = new ArticleInfo();
			testInfo.Name = "Test";
			testInfo.Type = ArticleType.Tool;
			var testArticle = new ArticleModel(testInfo);
			testArticle.Path = @"C:\dev\Testing";
			m_ArticleQueue.Add(testArticle);
			ArticleList.Add(testArticle);
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
							var childArticle = ArticleFactory.CreateArticle(x);
							AddToQueue(childArticle);
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
			if (m_ArticleQueue != null &&
				m_ArticleQueue.Count > 0)
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
			m_ArticleQueue.Clear();
			ArticleList.Clear();
		}
		public bool CanOpenMould(object context)
		{
			var item = context as ArticleModel;
			if (item?.Type == ArticleType.Plastic)
				return false;
			if (!File.Exists(item.GetMouldPath()))
				return false;
			return true;
		}
		public void OpenMould(object? context)
		{
			var article = context as ArticleModel;
			if (article == null) return;

			try
			{
				article.OpenMould();
			}
			catch (Exception e)
			{
				MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		public void OpenFolder(object? context)
		{
			var article = context as ArticleModel;
			if (article == null) return;

			try
			{
				article.OpenFolder();
			}
			catch (Exception e)
			{
				MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		public void OpenInfo(object? context)
		{
			var article = context as ArticleModel;
			if (article == null) return;

			try
			{
				article.OpenInfo();
			}
			catch (Exception e)
			{
				MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		public void RemoveFromQueue(object? context)
		{
			var item = context as ArticleModel;

			if (item == null)
				return;

			m_ArticleQueue.Remove(item);
			ArticleList.Remove(item);
		}

		private void OpenArticles()
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
		private void AddToQueue(ArticleModel article)
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
