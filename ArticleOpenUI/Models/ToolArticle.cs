using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
	public class ToolArticle : ArticleBase
	{
		private string m_Name = "";
		private string m_Path;
		private string m_Url;
		private string m_Cad;
		private string m_Customer;
		private string m_Shrinkage;
		private string m_Machine;
		private ArticleType m_Type;
		private List<string> m_Children;

		public override string Name => m_Name;
		public override string Url => m_Url;
		public override string Path => m_Path;
		public override string Cad => m_Cad;
		public override string Customer => m_Customer;
		public override string Shrinkage => m_Shrinkage;
		public override string Machine => m_Machine;
		public override ArticleType Type => m_Type;
		public override List<string> Children => m_Children;

		public ToolArticle(ArticleInfo info)
		{
			m_Name = info.Name;
			m_Path = info.Path;
			m_Url = info.URL;
			m_Type = info.Type;
			m_Cad = info.CAD;
			m_Customer = info.Customer;
			m_Shrinkage = info.Shrinkage;
			m_Machine = info.Machine;

			m_Children = new List<string>();
			if (info.Plastics != null && info.Plastics.Any())
				foreach (var child in info.Plastics)
					m_Children.Add(child);

			if (!Directory.Exists(Path))
				throw new DirectoryNotFoundException($"\"{Path}\" does not exist.");
		}
	}
}
