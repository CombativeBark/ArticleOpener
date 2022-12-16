using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Windows;

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
                    return $@"\\server1\ArticleFiles\ArtikelFiler\{Name.Substring(0, 7)}";
                else 
                    return $@"\\server1\ArticleFiles\ArtikelFiler\{Name}\{Name}";
            }
        }

        public ToolArticle(string name)
        {
            Name = name;
            _isModification = IsModification(Name);

            _children = GetChildren();

        }

        public override List<PlasticArticle> GetChildren()
        {
            List<PlasticArticle> result = new();

            HtmlDocument htmlDoc = new HtmlDocument();
            HtmlWeb htmlWeb = new HtmlWeb();
            htmlDoc = htmlWeb.Load(Url);

            throw new NotImplementedException();
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
