using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace ArticleOpenUI.Models
{
    public class ArticleInfo
    {
        public string Name { get; private set; } = "Unknown";
        public string URL { get; private set; } = "Unknown";
        public string Customer { get; private set; } = "Unknown";
        public string Description { get; private set; } = "Unknown";
        public string Material { get; private set; } = "Unknown";
        public string Shrinkage { get; private set; } = "Unknown";
        public string Machine { get; private set; } = "Unknown";
        public ArticleType Type { get; private init; }
        public List<string>? Plastics { get; init; } = null;

        public ArticleInfo(string name)
        {
            var doc = new HtmlDocument();
            var web = new HtmlWeb();

            Name = name;
            Type = GetArticleType();
            URL = GetURL();

            doc = web.Load(URL);
            if (doc != null)
            {
                var body = doc.DocumentNode.SelectSingleNode("//body");
                if (body != null)
                {
                    var nodes = body.SelectNodes("//div//div")
                        .Where(x => x.FirstChild.Name == "h1")
                        .ToList();

                    HtmlNode prevNode;
                    for (int i = 0; i < nodes[0].ChildNodes.Count; i++)
                    {
                        HtmlNode? node = nodes[0].ChildNodes[i];
                        var nextNode = nodes[0].ChildNodes[i+1];
                        switch (node.InnerText.ToLower())
                        {
                            case ("customer"):
                                Customer = nextNode.SelectNodes("//td")[0].InnerText;
                                break;
                            case ("plastic"):
                                Material = nextNode.InnerText;
                                break;
                            case ("2110"):
                                Machine = nextNode.InnerText;
                                break;
                            case ("plastics"):
                                
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private ArticleType GetArticleType()
        {
            const string toolPattern = @"^\d{6}V\d?$";
            const string plasticPattern = @"^\d{6}P(?:-\d)?$";

            if (Regex.IsMatch(Name, toolPattern, RegexOptions.Compiled))
            {
                return ArticleType.Tool;
            }
            else if (Regex.IsMatch(Name, plasticPattern, RegexOptions.Compiled))
            {
                return ArticleType.Plastic;
            }
            else
            {
                throw new ArgumentException($"Couldn't find type for article {Name}");
            }
        }
        private string GetURL()
        {
            switch (Type)
            {
                case ArticleType.Tool:
                    return $@"http://server1:85/tool/{Name}";
                case ArticleType.Plastic:
                    return $@"http://server1:85/plastic/{Name}";
                default:
                    throw new ArgumentException($"Article type of {Name} is not support");
            }
        }

        private bool IsPlasticsNode(HtmlNode node) 
        {
            const string plasticPattern = @"^\d{6}P(?:-\d)?$";
            var output = Regex.IsMatch(node.InnerText, plasticPattern, RegexOptions.Compiled);
            return output; 
        }

        private List<string> GetPlastics()
        {
            var output = new List<string>();

            return output;
        }
    }
}