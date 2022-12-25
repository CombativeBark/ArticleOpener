using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace ArticleOpenUI.Models
{
	public class ToolArticle : ArticleBase
	{
		private bool _isModification = false;
		private List<PlasticArticle> _children;

		public override ArticleType Type => ArticleType.Tool;
		public override string Url => $@"http://server1:85/tool/{Name}";
		public override string Path
		{ 
			get 
			{ 
				if (_isModification)
					return $@"\\server1\ArtikelFiler\ArticleFiles\{Name.Substring(0, 7)}";
				else 
					return $@"\\server1\ArtikelFiler\ArticleFiles\{Name}\{Name}";
			}
		}
		public override List<PlasticArticle> Children { get => _children; }

		public ToolArticle(string name)
		{
			if (IsNameValid(name))
            {
                Name = name;
            }


            _isModification = IsModification(Name);

			if (!Directory.Exists(Path))
				throw new DirectoryNotFoundException($"\"{Path}\" does not exist.");

			_children = GetChildren();
		}
		public override List<PlasticArticle> GetChildren()
		{
			List<PlasticArticle> result = new();

			HtmlDocument htmlDoc = new HtmlDocument();
			HtmlWeb htmlWeb = new HtmlWeb();
			htmlDoc = htmlWeb.Load(Url);

			if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
			{
				throw new HtmlWebException($"{htmlDoc.ParseErrors.Count()} errors occured while trying to parse html");
			}
			else
			{
				if (htmlDoc.DocumentNode != null)
				{
					HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
					if (bodyNode != null)
					{
						const string pattern = @"^(?:plastic/)(\d{6}P(?:-\d)?)$";

						var children = bodyNode.SelectNodes("//td/a")
							.Where(x => Regex.IsMatch(x.Attributes["href"].Value, pattern));

						children.ToList<HtmlNode>().ForEach(x =>
						{
                            string plasticName = x.Attributes["href"].Value.Replace("plastic/", "");
                            PlasticArticle plastic = (PlasticArticle)ArticleFactory.CreateArticle(plasticName);

							if (plastic != null)
							{
								result.Add(plastic);
							}
						});


					}
				}
			}

			return result;
		}

		public bool IsModification(string name)
		{
			const string regex = @"^\d{6}V[1-9]$";

			if (Regex.IsMatch(name, regex, RegexOptions.Compiled))
				return true;

			return false;
		}
	}
}
