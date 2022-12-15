using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
    internal interface IArticle
    {
        string Name { get; set; }
        ArticleType Type { get; set; }
        string Path { get; set; }
        string Url { get; set; }

        public void OpenFolder()
        {
			if (Directory.Exists(Path))
			{
				ProcessStartInfo startInfo = new ProcessStartInfo()
				{
					Arguments = Path,
					FileName = "explorer.exe"
				};

				Process.Start(startInfo);
			}
			else
            {
                throw new Exception($"Error: Directory for Article {Name} doesn't exist");
            }
        }

        public void OpenInfo()
        {
			ProcessStartInfo startInfo = new()
			{
				FileName = Url,
				UseShellExecute = true,
			};
			Process.Start(startInfo);
        }

        private bool IsNameValid(string name)
        {
            const string regex = @"^\d{6}[VP](?:\d|-\d)?$";

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Error: Article Number is null or empty.");

            if (Regex.IsMatch(name, regex, RegexOptions.Compiled))
            {
                return true;
            }
            else
                throw new ArgumentException($"Error: {name} is not a valid Article Number");
        }

        private string GetPath()
        {
			string rootPath = @"\\Server1\ArtikelFiler\ArticleFiles";
            
            switch (Type)
            {
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
            }
        }
        private string GetUrl()
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
    }
}
