using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArticleOpenUI.Models;

namespace ArticleOpenUI.Helpers
{
	public static class ArticleFactory
	{
		public async static Task<ArticleModel> CreateArticle(string input)
		{
			var articleName = input.ToUpper();
			var info = new ArticleInfo(articleName);

			if (info == null)
				throw new ArgumentException($"Error: Can't process info for article {articleName}");

			return new ArticleModel(info);
		}
		public static ArticleModel CreateArticle(ArticleInfo inputInfo)
		{
			if (inputInfo == null)
				throw new ArgumentNullException(nameof(inputInfo));

			return new ArticleModel(inputInfo);
		}
		public async static Task<List<ArticleModel>> CreateArticles(List<string> listOfInputs)
		{
			var result = new List<ArticleModel>();
			var taskList = new List<Task<ArticleModel>>();

            foreach (var input in listOfInputs)
            {
				var response = CreateArticle(input);
				taskList.Add(response);
            }
			
			await Task.WhenAll(taskList);
			taskList.ForEach(task => result.Add(task.Result));
			return result;
		}
	}
}