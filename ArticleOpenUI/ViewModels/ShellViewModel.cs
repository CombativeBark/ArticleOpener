using ArticleOpenUI.Models;
using Caliburn.Micro;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace ArticleOpenUI.ViewModels
{

    class ShellViewModel : Screen
    {
		private List<ArticleModel> _articles = new();
		private string _input = "";
		private string _displayedArticles;

		public string Input
		{
			get { return _input; }
			set
			{
				_input = value;
				NotifyOfPropertyChange(() => Input);
			}
		}
        public string DisplayedArticles 
		{
			get { return _displayedArticles; }
			set
			{
				_displayedArticles = value;
				NotifyOfPropertyChange(() => DisplayedArticles);
			} 
		}

		public void SearchArticle()
		{
			var articleNumbers = ParseInput(Input);
			foreach ( var artNumber in articleNumbers )
			{
				_articles.Add(new ArticleModel(artNumber));
			}

			UpdateArticleNumberDisplay();
		}

        List<string> ParseInput(string input)
		{
			List<string> result = new();

			var splitInput = input.Split(new char[] { ' ', '.', ':', ',', ';' });
			foreach ( var item in splitInput )
			{
				result.Add(item);
			}

			return result;
		}

		void UpdateArticleNumberDisplay()
		{
			DisplayedArticles = "";
			foreach ( var article in _articles)
			{
				DisplayedArticles += article.Name + "\n";
			}
		}
	}
}
