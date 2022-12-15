using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleOpenUI.Models
{
	class Tool : ArticleBase
	{
		public override IArticle CreateArticle(string name)
		{
			Tool tool = new Tool();

			if (IsNameValid(name))
			{
				tool.Name = name;
			}

			tool.Type = ArticleType.Tool;
			tool.Path = GetPath();
			tool.Url = GetUrl();
			
			return tool;
		}

		public override List<IArticle> GetChildren()
		{
			throw new NotImplementedException();
		}

		/*
			case ArticleType.Tool:
				return $@"{rootPath}\{Name}\{Name}";
			case ArticleType.Modification:
				return $@"{rootPath}\{Name.Substring(0,7)}";
			case ArticleType.Plastic:
				return $@"{rootPath}\{Name}\{Name}";
			case ArticleType.PlasticVariant:
				return $@"{rootPath}\{Name.Substring(0,7)}\{Name.Substring(0,7)}";
			default:
				throw new Exception($"Error: Article {Name} doesn't have a type");
		*/
		private protected override string GetPath()
		{
			throw new NotImplementedException();
		}

		/*
		{
			string baseUrl = @"http://server1:85";

			if (Type == ArticleType.Tool)
			{
				return $@"{baseUrl}\{Name}\{Name}";
			}
			else if (Type == ArticleType.Modification)
			{
				return $@"{baseUrl}\{Name.Substring(0, 7)}";
			}
			else if (Type == ArticleType.Plastic || Type == ArticleType.PlasticVariant)
			{
				return $@"{baseUrl}/plastic/{Name}";
			}
			else
			{
				throw new Exception($"Error: Article {Name} doesn't have a type");
			}
		}
		*/
		private protected override string GetUrl()
		{
			throw new NotImplementedException();
		}
	}
}
