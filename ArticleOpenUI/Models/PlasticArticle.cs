using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
	public class PlasticArticle : ArticleBase
	{
		private string m_Name;
		private string m_Path;
		private string m_Url;
		private string m_Customer;
		private string m_Description;
		private string m_Material;
		private string m_Shrinkage;
		private string m_Machine;
		private ArticleType m_Type;

		public override string Name => m_Name;
		public override string Url => m_Url;
		public override string Path => m_Path;
		public override string Customer => m_Customer;
		public override string Description => m_Description;
		public override string Material => m_Material;
		public override string Shrinkage => m_Shrinkage;
		public override string Machine => m_Machine;
		public override ArticleType Type => m_Type;
		public override List<string>? Children => null;

		public PlasticArticle(ArticleInfo info)
		{
			m_Name = info.Name;
			m_Type = info.Type;
			m_Path = info.Path;
			m_Url = info.URL;
			m_Customer = info.Customer;
			m_Description = info.Description;
			m_Material = info.Material;
			m_Shrinkage = info.Shrinkage;
			m_Machine = info.Machine;
		}
	}
}