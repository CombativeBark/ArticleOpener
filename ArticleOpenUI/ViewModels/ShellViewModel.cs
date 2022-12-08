using ArticleOpenUI.Models;
using Caliburn.Micro;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ArticleOpenUI.ViewModels
{

    class ShellViewModel : Screen
    {
		private List<ArticleModel> _articleQueue = new();
		private string _inputText = "";
		private string _queueDisplay = "Article";

		public string InputText
		{
			get { return _inputText; }
			set
			{
				_inputText = value;
				NotifyOfPropertyChange(() => InputText);
			}
		}
        public string QueueDisplay 
		{
			get { return _queueDisplay; }
			set
			{
				_queueDisplay = value;
				NotifyOfPropertyChange(() => QueueDisplay);
			} 
		}
		public ObservableCollection<ArticleModel> Articles { get; private set; }

		public ShellViewModel()
		{
			Articles = new();
		}

		public void SearchArticle(string? input)
		{
			if (string.IsNullOrEmpty(input))
				input = InputText;
			
			var inputList = SplitString(input);
			
			foreach (var articleNumber in inputList)
			{
				if (string.IsNullOrWhiteSpace(articleNumber))
					continue;

				if (!IsInQueue(articleNumber))
				{
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

					_articleQueue.Add(article);
					Articles.Add(article);
				}
				
			}

			UpdateQueueDisplay();
		}

		public void OpenArticles()
		{
			if (_articleQueue.Count > 0)
				Open(ArticleOpenMode.All);
			else
			{
				MessageBox.Show("No articles to open");
			}
		}

		private void Open(ArticleOpenMode openMode)
		{
			foreach (var article in _articleQueue)
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

        private bool IsInQueue(string inputArticle)
        {
			foreach (var article in _articleQueue)
			{
				if (article.Name.Equals(inputArticle))
					return true;
			}
			return false;
        }

        List<string> SplitString(string input)
		{
			List<string> result = new();

			var splitString = input.Split(new char[] { ' ', '.', ':', ',', ';' });
			foreach (var substring in splitString)
			{
				result.Add(substring);
			}

			return result;
		}

		void UpdateQueueDisplay()
		{
			QueueDisplay = string.Empty;
			foreach (var article in _articleQueue)
			{
				QueueDisplay += article.Name + "\n";
			}
		}
	}
}
