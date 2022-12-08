using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ArticleOpenUI.ViewModels
{

    class ShellViewModel : Screen
    {
		private List<ArticleModel> m_ArticleQueue;
		private string m_Input = "";

		public string Input
		{
			get { return m_Input; }
			set
			{
				m_Input = value;
				NotifyOfPropertyChange(() => Input);
			}
		}
		public ObservableCollection<ArticleModel> ArticleData { get; private set; }

		public ShellViewModel()
		{
			m_ArticleQueue = new();
			ArticleData = new();
		}
		public void SearchArticle(string input)
		{
			if (string.IsNullOrEmpty(input))
				input = Input;
			
			foreach (var articleNumber in SplitString(input))
			{
				if (string.IsNullOrWhiteSpace(articleNumber) || IsInQueue(articleNumber))
					continue;

				var article = ArticleModel.CreateArticle(articleNumber);

				if (article == null)
					continue;

				if (article.Type == ArticleType.Tool)
				{
					foreach (var child in article.GetChildren())
					{
						SearchArticle(child);
					}
				}

				AddToQueue(article);
			}
		}
		public void OpenArticlesInQueue()
		{
			if (m_ArticleQueue.Count > 0)
				OpenArticle(ArticleOpenMode.All);
			else
			{
				MessageBox.Show("No articles to open");
			}
		}
		public void ClearQueue()
		{
			m_ArticleQueue.Clear();
			ArticleData.Clear();
		}

		private void OpenArticle(ArticleOpenMode openMode)
		{
			foreach (var article in m_ArticleQueue)
			{
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
			}
		}
		private void AddToQueue(ArticleModel article)
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
