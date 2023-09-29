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
		public abstract string Name { get; set; }
		public abstract string Url { get; set; }
		public abstract string Path { get; set; }
		public abstract string Customer { get; set; }
		public abstract string Machine { get; set; }
		public abstract string Shrinkage { get; set; }
		public abstract bool IsSpecial { get; set; }

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
		public void OpenInfo()
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
