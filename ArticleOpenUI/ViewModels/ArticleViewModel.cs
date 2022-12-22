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

					if (article.Type == ArticleType.Tool)
					{
						article.GetChildren().ForEach(x => AddToQueue(x));
					}

					AddToQueue(article);
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message + "\n\n" + e.StackTrace);
				}

			}
		}
		public void OpenArticlesInQueue()
		{
			if (m_ArticleQueue.Count > 0)
				OpenArticles(ArticleOpenMode.All);
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

		private void OpenArticles(ArticleOpenMode openMode)
		{
			foreach (var article in m_ArticleQueue)
			{
				/*
				switch (openMode)
				{
					case ArticleOpenMode.All:
						article.OpenAll();
						break;
					case ArticleOpenMode.FoldersOnly: 
						article.OpenFolder();
						break;
					case ArticleOpenMode.InfoOnly:
						article.OpenInfo();
						break;
					case ArticleOpenMode.ToolsOnly: 
						if (article.Type == ArticleType.Tool || article.Type == ArticleType.Modification)
							article.OpenAll();
						break;
					case ArticleOpenMode.PartsOnly: 
						if (article.Type == ArticleType.Plastic || article.Type == ArticleType.PlasticVariant)
							article.OpenAll();
						break;
					case ArticleOpenMode.None:
						MessageBox.Show($"Can't open {article.Name} because it doesn't have a type");
						break;
					default:
						throw new NotImplementedException();

				}
				*/
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
