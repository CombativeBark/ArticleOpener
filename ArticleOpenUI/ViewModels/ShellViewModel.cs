using ArticleOpenUI.Models;
using Caliburn.Micro;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
				if (IsNewArticle(number))
					_articleQueue.Add(new ArticleModel(number));
			}

			UpdateQueueDisplay();
		}

        private bool IsNewArticle(string articleNumber)
        {
			foreach (var article in _articleQueue)
			{
				if (article.Name.Equals(articleNumber))
					return false;
			}
			return true;
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
