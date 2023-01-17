using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
    public abstract class ArticleBase
    {
        private string m_Name = "";
        private ArticleType m_Type;

        public virtual string Name { get => m_Name; set => m_Name = value; }
        public virtual ArticleType Type { get => m_Type; private set => m_Type = value; }
        public abstract string Path { get; }
        public abstract string Url { get; }
        public abstract List<PlasticArticle> Children { get; }

        public abstract List<PlasticArticle> GetChildren();

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

        public bool IsNameValid(string name)
        {
            const string regex = @"^\d{6}[VP](?:\d|-\d)?$";

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Error: Article Number is null or empty.");
            }

            if (Regex.IsMatch(name, regex, RegexOptions.Compiled))
            {
                return true;
            }
            else
            {
                throw new ArgumentException($"Error: {name} is not a valid Article Number");
            }
        }
    }
}
