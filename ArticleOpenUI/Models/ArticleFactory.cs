using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
    public static class ArticleFactory
    {
		/* input string
		 * split/parse string
		 * error on bad input
		 * 
		 * try to create articles
		 * create tools
		 * populate tools with info
		 * if tool has children
		 * create plastics
		 * populate plastics with info
		 * return articles
		 */
		
        public static ArticleBase CreateArticle(string name)
        {
			var info = GetInfo(name);

			if (info == null)
				throw new ArgumentException($"Error: Can't process article number {name}");


			switch (info.Type)
			{
				case ArticleType.Tool:
                    var toolArticle = new ToolArticle(info);
                    return toolArticle;
				case ArticleType.Plastic:
					var plasticArticle = new PlasticArticle(info);
					return plasticArticle;
				default:
					throw new Exception($"Error: {name} is not a supported article type");
			}
        }

        private static ArticleType? GetType(string name)
        {
			Dictionary<ArticleType, string> regexPatterns = new Dictionary<ArticleType, string>()
			{
				{ ArticleType.Tool, @"^\d{6}V\d?$" },
				{ ArticleType.Plastic, @"^\d{6}P(?:-\d)?$" },
			};

			foreach (var pattern in regexPatterns)
			{
				if (Regex.IsMatch(name, pattern.Value, RegexOptions.Compiled))
				{
					return pattern.Key;
				}
			}

			return null;
        }

		private static ArticleInfo GetInfo(string name)
		{
			var output = new ArticleInfo(name);

			return output;
		}
    }
}
