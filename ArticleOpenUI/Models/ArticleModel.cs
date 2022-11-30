using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace ArticleOpenUI.Models
{
    enum ArticleType
	{
        None,
		Tool,
		Modification,
		Plastic,
		PlasticVariant
	}

    class ArticleModel
    {
        private string _name = "No Number";
        private ArticleType _type = ArticleType.None;
        private string _path = "No Path";
        private string _url = "No URL";

        public string Name 
        { 
            get => _name;
            set
            {
                if (CheckNameValidity(value))
                    _name = value; 
            }
        }
        public ArticleType Type { get; set; }
        public string Path { get; set; }
        public string URL { get; set; }

        public ArticleModel(string name)
        {
            Name = name;
            try
            {
                Type = GetArticleType();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Path = GetPath();
            URL = GetURL();
        }
        private bool CheckNameValidity(string name)
        {
            const string REGEX_ARTICLE = @"^\d{6}[VP][1-9]?-?\d?$";
            return Regex.IsMatch(name, REGEX_ARTICLE, RegexOptions.Compiled);

        }

        private ArticleType GetArticleType()
        {
            const string REGEX_PLASTIC = @"^\d{6}P-?\d?$";
            const string REGEX_PLASTIC_VARIANT = @"^\d{6}P-\d$";
            const string REGEX_TOOL = @"^\d{6}V[1-9]?$";
            const string REGEX_MODIFICATION = @"^\d{6}V\d$";

            if (Regex.IsMatch(Name, REGEX_PLASTIC, RegexOptions.Compiled))
            {

                if (Regex.IsMatch(Name, REGEX_PLASTIC_VARIANT, RegexOptions.Compiled))
                    return ArticleType.PlasticVariant;
                else
                    return ArticleType.Plastic;
            }
            else if (Regex.IsMatch(Name, REGEX_TOOL, RegexOptions.Compiled))
            {
                if (Regex.IsMatch(Name, REGEX_MODIFICATION, RegexOptions.Compiled))
                    return ArticleType.Modification;
                else
                    return ArticleType.Tool;
            }
            else
                throw new ArgumentException();
        }

        private string GetPath()
        {
            string rootPath = @"\\Server1\ArtikelFiler\ArticleFiles";
            string path = $@"{rootPath}\{Name}\{Name}";
            string fullPath = path;
            char[] trimNumbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };


            if (Type == ArticleType.Modification)
            {
                path = $@"{rootPath}\{Name.TrimEnd(trimNumbers)}";
                fullPath = $@"{path}\{Name}";
            }
            else if (Type == ArticleType.PlasticVariant)
            {
                path = $@"{rootPath}\{Name}";
                fullPath = path;
            }
            if (Directory.Exists(fullPath))
                return path;
            else
                return "Path doesn't exist";
        }

        private string GetURL()
        {
            string baseURL = @"http://server1:85";

            if (Type == ArticleType.Plastic || Type == ArticleType.PlasticVariant)
                return $@"{baseURL}/plastic/{Name}";
            else
                return $@"{baseURL}/tool/{Name}";
        }

        public void PrintInfo()
        {
            MessageBox.Show($"Article Info\n\tName: {Name}\n\tType: {Type}\n\tPath: {Path}\n\tURL: {URL}");
        }
    }
}
