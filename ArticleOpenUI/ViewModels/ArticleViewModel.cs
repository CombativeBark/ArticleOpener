using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ArticleOpenUI.ViewModels
{

	class ArticleViewModel : Screen
	{
		private List<ArticleBase> m_ArticleQueue;
		private string m_Input;
		private string m_InputError;

		public string Input
		{
			get { return m_Input; }
			set
			{
				m_Input = value;
				NotifyOfPropertyChange(() => Input);
			}
		}
		public string InputError { get; set; }
		public ObservableCollection<ArticleBase> ArticleData { get; private set; }

		public ArticleViewModel()
		{
			m_ArticleQueue = new();
			ArticleData = new();
			m_Input = "";
			m_InputError = "";
		}
		public void SearchArticle(string input)
		{
			if (string.IsNullOrEmpty(input))
				input = Input;

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

		private void OpenArticles(ArticleOpenMode openMode, ArticleOpenFilter openFilter)
		{
			m_ArticleQueue.ForEach(article =>
			{
				bool isSkipped = FilterArticle(article, openFilter);

				if (isSkipped)
					return;

				switch (openMode)
				{
					case ArticleOpenMode.All:
						article.OpenFolder();
						article.OpenInfo();
						break;
					case ArticleOpenMode.Folders:
						article.OpenFolder();
						break;
					case ArticleOpenMode.Info:
						article.OpenInfo();
						break;
					default: throw new ArgumentOutOfRangeException("Please select which option to open.");
				}
				
			});
		}
		private bool FilterArticle(ArticleBase article, ArticleOpenFilter filter)
		{
			switch (filter)
			{
				case ArticleOpenFilter.All:
					return false;
				case ArticleOpenFilter.Tools:
					if (article.Type == ArticleType.Tool)
						return false;
					else
						return true;
				case ArticleOpenFilter.Parts:
					if (article.Type == ArticleType.Plastic)
						return false;
					else
						return true;
				default: throw new ArgumentOutOfRangeException("Please select which type to open.");
			}
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
