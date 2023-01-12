using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArticleOpenUI.ViewModels
{

	class ArticleViewModel : Screen
	{
		private List<ArticleBase> m_ArticleQueue;
		private string m_Input;
		private bool m_OpenTools;
		private bool m_OpenPlastics;
		private bool m_OpenInfo;
		private bool m_OpenFolders;

		public string Input
		{ 
			get { return m_Input; }
			set
			{
				m_Input = value;
				NotifyOfPropertyChange(() => Input);
			}
		}
		public bool OpenTools 
		{ 
			get
			{
				return m_OpenTools;
			}
			set
			{
				m_OpenTools = value;
				NotifyOfPropertyChange(() => OpenTools);
			} 
		}
		public bool OpenPlastics 
		{ 
			get
			{
				return m_OpenPlastics;
			}
			set
			{
				m_OpenPlastics = value;
				NotifyOfPropertyChange(() => OpenPlastics);
			} 
		}
		public bool OpenInfo 
		{ 
			get
			{
				return m_OpenInfo;
			}
			set
			{
				m_OpenInfo = value;
				NotifyOfPropertyChange(() => OpenInfo);
			} 
		}
		public bool OpenFolders 
		{ 
			get
			{
				return m_OpenFolders;
			}
			set
			{
				m_OpenFolders = value;
				NotifyOfPropertyChange(() => OpenFolders);
			} 
		}
		public ObservableCollection<ArticleBase> ArticleData { get; private set; }

		public ArticleViewModel()
		{
			m_ArticleQueue = new();
			m_Input = "";

			OpenTools = true;
			OpenPlastics = true;
			OpenInfo = true;
			OpenFolders = true;
			ArticleData = new();
		}
		public void SearchArticle(string input)
		{
			if (string.IsNullOrEmpty(input))
				input = Input;

#if (DEBUG)
			var activeOptions = "";

			if (OpenTools)
				activeOptions += "FilterTools ";
			if (OpenPlastics)
				activeOptions += "FilterPlastics ";
			if (OpenInfo)
				activeOptions += "OpenUrls ";
			if (OpenFolders)
				activeOptions += "OpenFolders ";

			MessageBox.Show(activeOptions);
#endif

			foreach (var articleNumber in SplitString(input))
			{
				if (string.IsNullOrWhiteSpace(articleNumber) || IsInQueue(articleNumber))
                    continue;

				try
				{
					var article = ArticleFactory.CreateArticle(articleNumber);

					AddToQueue(article);

					if (article.Type == ArticleType.Tool)
					{
						article.Children.ForEach(x => AddToQueue(x));
					}
				}
				catch (Exception e)
				{
					#if (DEBUG)
					MessageBox.Show($"{e.Message}\n\n{e.StackTrace}" , "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					#else
					MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
		public void RemoveItem(ArticleBase article)
		{
			m_ArticleQueue.Remove(article);
		}
		public void ClearQueue()
		{
			m_ArticleQueue.Clear();
			ArticleData.Clear();
		}
		public void TextBoxEvent(ActionExecutionContext context)
		{
			var keyArgs = context.EventArgs as KeyEventArgs;

			if (keyArgs == null && string.IsNullOrWhiteSpace(Input))
				return;

            switch (keyArgs.Key)
            {
                case Key.Enter:
                    SearchArticle(Input);
                    break;
                case Key.Escape:
                    ClearQueue();
                    break;
                default:
                    break;
            }
        }
		private void OpenArticles(ArticleOpenMode openMode, ArticleOpenFilter openFilter)
		{
			m_ArticleQueue.ForEach(article =>
			{
				bool isSkipped = FilterArticle(article, openFilter);

				if ((!OpenTools && article.Type == ArticleType.Tool) ||
				(!OpenPlastics && article.Type == ArticleType.Plastic))
					return;

				if (OpenFolders)
				{
						article.OpenFolder();
				}

				if (OpenInfo)
                {
						article.OpenInfo();
					Thread.Sleep(100);
				}
				
			});
		}
		private void AddToQueue(ArticleBase article)
		{
			m_ArticleQueue.Add(article);
			ArticleData.Add(article);
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

			var splitString = input.Split(new char[] { ' ', '.', ':', ',', ';' });
			foreach (var substring in splitString)
			{
				result.Add(substring);
			}

			return result;
		}
	}
}
