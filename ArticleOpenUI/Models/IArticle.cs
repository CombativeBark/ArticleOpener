using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArticleOpenUI.Models
{
    internal interface IArticle
    {
        public string Name { get; set; }
        public ArticleType Type { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }

        public IArticle CreateArticle(string name);
        public List<IArticle> GetChildren();
        public void OpenFolder();
        public void OpenInfo();
    }
}
