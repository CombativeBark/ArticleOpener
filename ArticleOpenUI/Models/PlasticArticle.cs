using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleOpenUI.Models
{
	public class PlasticArticle : ArticleBase
	{
		private bool _isVariant = false;

		public override ArticleType Type => ArticleType.Plastic;
		public override string Url => $@"http://server1:85/plastic/{Name}";
		public override string Path
		{ 
			get 
			{ 
				if (_isVariant)
					return $@"\\server1\ArtikelFiler\ArticleFiles\{Name.Substring(0, 7)}";
				else 
					return $@"\\server1\ArtikelFiler\ArticleFiles\{Name}\{Name}";
			}
		}
        public override List<PlasticArticle> Children => new List<PlasticArticle>();

        public PlasticArticle(string name)
		{
			Name = name;

			_isVariant = IsVariant(Name);

			if (!Directory.Exists(Path))
				throw new DirectoryNotFoundException($"\"{Path}\" does not exist.");
		}

		public override List<PlasticArticle> GetChildren()
		{
			return new List<PlasticArticle>{ };
		}

		private bool IsVariant(string name)
		{
			const string regex = @"^\d{6}P-\d$";

			if (Regex.IsMatch(name, regex, RegexOptions.Compiled))
				return true;

			return false;
		}
	}
}