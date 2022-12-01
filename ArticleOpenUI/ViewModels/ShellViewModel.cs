using ArticleOpenUI.Models;
using Caliburn.Micro;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace ArticleOpenUI.ViewModels
{

    class ShellViewModel : Screen
    {
		private List<ArticleModel> _articleQueue = new();
		private string _inputText = "";
		private string _queueDisplay = "";

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

		public void SearchArticle()
		{
			var articleNumbers = SplitString(InputText);
			
			foreach (var number in articleNumbers)
			{
				if (!IsInQueue(number))
					_articleQueue.Add(new ArticleModel(number));
			}

			UpdateQueueDisplay();
		}

		public void OpenAll()
		{
			Open(ArticleOpenMode.All);
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
