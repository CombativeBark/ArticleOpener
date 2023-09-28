using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleOpenUI.Models.Article
{
	public abstract class ArticleBase : IArticle
	{
		private bool isModOrVariant;

		private string m_Name;
		public string Name { get => m_Name; set => m_Name = value; }
		private string m_Url;
		public string Url { get => m_Url; set => m_Url = value; }
		private string m_Path;
		public string Path { get => m_Path; set => m_Path = value; }
		private string m_Customer;
		public string Customer { get => m_Customer; set => m_Customer = value; }
		private string m_Description;
		public string Description { get => m_Description; set => m_Description = value; }
		private string m_Machine;
		public string Machine { get => m_Machine; set => m_Machine = value; }
		private string m_Material;
		public string Material { get => m_Material; set => m_Material = value; }
		private string m_Shrinkage;
		public string Shrinkage { get => m_Shrinkage; set => m_Shrinkage = value; }

		private ArticleBase()
        {
			// Not allowed to instatiate base class    
        }

        public void OpenFolder()
		{
            if (!Directory.Exists(Path))
                throw new DirectoryNotFoundException($"Directory for Article {Name} doesn't exist");

            var startInfo = new ProcessStartInfo()
            {
                Arguments = Path,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
		}
		public void OpenInfoPage()
		{
            if (Url == null || string.IsNullOrWhiteSpace(Url))
                throw new ArgumentNullException(Url, $"URL for Article {Name} is missing.");

            ProcessStartInfo startInfo = new()
            {
                FileName = Url,
                UseShellExecute = true,
            };
            Process.Start(startInfo);
		}
	}
}
