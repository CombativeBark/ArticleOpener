using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArticleOpenUI.Models.Article;

namespace ArticleOpenUI.Models
{
    public class ArticleInfoModel
    {
		public string Name { get; set; } = "";
		public ArticleType Type { get; set; }
		public bool IsModOrVariant { get; set; } = false;
		public string Url { get; set; } = ""; 
		public List<string> Plastics { get; set; } = new List<string>();
		public string CAD { get; set; } = "";
		public string Customer { get; set; } = "";
		public string Description { get; set; } = "";
		public string Material { get; set; } = "";
		public Dictionary<string, string> Shrinkage { get; set; } = new Dictionary<string, string>();
		public string Machine { get; set; } = "";

		private ArticleInfoModel()
        {
            
        }
        public ArticleInfoModel(string name)
        {
			if (name is not null && IsValidName(name))
				Name = name;
			else 
				throw new ArgumentNullException("name");

			Type = ResolveType();
			Url = ResolveURL();
		}

		private ArticleType ResolveType()
		{
			var reResult = Regex.Match(Name, @"\d{6}(?<Type>[PV])(?<Modification>.*)", RegexOptions.IgnoreCase);

			IsModOrVariant = string.IsNullOrWhiteSpace(reResult.Groups[2].ToString());

			return 0;

			{
				if (Regex.IsMatch(Name, @"^\d{6}[VP](?:\d|-\d)$"))
					IsModOrVariant = true;

				if (Regex.IsMatch(Name, @"^\d{6}V\d?$"))
					return ArticleType.Tool;
				else if (Regex.IsMatch(Name, @"^\d{6}P(?:-\d)?$"))
					return ArticleType.Plastic;
				else
					throw new ArgumentException($"Couldn't find type for article {Name}");
			}
		}

		private string ResolveURL()
		{
			return Type switch
			{
				ArticleType.Tool => $@"http://server1:85/tool/{Name}",
				ArticleType.Plastic => $@"http://server1:85/plastic/{Name}",
				_ => throw new ArgumentException($"Article type of {Name} is not support"),
			};
		}

		private bool IsValidName(string name) => Regex.IsMatch(name, @"\d{6}[VP](?:-?\d)?", RegexOptions.IgnoreCase);
    }
}
