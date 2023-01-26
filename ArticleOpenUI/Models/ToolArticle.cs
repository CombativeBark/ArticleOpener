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
using System.Reflection.PortableExecutable;

namespace ArticleOpenUI.Models
{
	public class ToolArticle : ArticleBase
	{
		private string m_Name = "";
		private string m_Url;
		private string m_Cad;
		private string m_Customer;
		private string m_Shrinkage;
		private string m_Machine;
		private ArticleType m_Type;
		private bool m_IsModification = false;
		private List<string> m_Children;

		public override string Name => m_Name;
		public override string Url => m_Url;
		public override string Path
		{ 
			get 
			{ 
				if (m_IsModification)
					return $@"\\server1\ArtikelFiler\ArticleFiles\{Name.Substring(0, 7)}";
				else 
					return $@"\\server1\ArtikelFiler\ArticleFiles\{Name}\{Name}";
			}
		}
		public override string Cad => m_Cad;
		public override string Customer => m_Customer;
		public override string Shrinkage => m_Shrinkage;
		public override string Machine => m_Machine;
		public override ArticleType Type => m_Type;
		public override List<string> Children => m_Children;


		public ToolArticle(ArticleInfo info)
		{
			if (info == null) 
				throw new ArgumentNullException(nameof(info));

			if (IsNameValid(info.Name))
                m_Name = info.Name;
            m_IsModification = IsModification(Name);

			m_Url = info.URL;
			m_Type = info.Type;
			m_Cad = info.CAD;
			m_Customer = info.Customer;
			m_Shrinkage = info.Shrinkage;
			m_Machine = info.Machine;

			m_Children = new List<string>();
			if (info.Plastics != null && info.Plastics.Any())
			{
				foreach (var child in info.Plastics)
				{
					m_Children.Add(child);
				}

			}


			if (!Directory.Exists(Path))
				throw new DirectoryNotFoundException($"\"{Path}\" does not exist.");

		}
		public List<PlasticArticle> GetChildren()
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
