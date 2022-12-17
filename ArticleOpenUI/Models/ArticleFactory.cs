using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
    public static class ArticleFactory
    {
        public static ArticleBase? CreateArticle(string name)
        {
			var type = GetType(name);

			if (type == null)
				throw new ArgumentException($"Error: Can't find a type for article {name}");

			switch (type)
			{
				case ArticleType.Tool:
					return new ToolArticle(name);
				case ArticleType.Plastic:
					return new PlasticArticle(name);

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
    }
}
