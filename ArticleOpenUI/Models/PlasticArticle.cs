using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
	public class PlasticArticle : ArticleBase
	{
		private string m_Name = "";
		private string m_Url;
		private string m_Customer;
		private string m_Description;
		private string m_Material;
		private string m_Machine;
		private ArticleType m_Type;
		private bool m_IsVariant = false;

		public override string Name => m_Name;
		public override string Url => m_Url;
		public override string Path
		{
			get
			{
				if (m_IsVariant)
					return $@"\\server1\ArtikelFiler\ArticleFiles\{Name.Substring(0, 7)}";
				else
					return $@"\\server1\ArtikelFiler\ArticleFiles\{Name}\{Name}";
			}
		}
		public override string Customer => m_Customer;
		public override string Description => m_Description;
		public override string Material => m_Material;
		public override string Machine => m_Machine;
		public override ArticleType Type => m_Type;
		public override List<string>? Children => null;

		public PlasticArticle(ArticleInfo info)
		{
			if (IsNameValid(info.Name))
				m_Name = info.Name;
			m_Type = info.Type;
			m_IsVariant = IsVariant(Name);

			m_Url = info.URL;
			m_Customer = info.Customer;
			m_Description = info.Description;
			m_Material = info.Material;
			m_Machine = info.Machine;


			if (!Directory.Exists(Path))
				throw new DirectoryNotFoundException($"\"{Path}\" does not exist.");
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