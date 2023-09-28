using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ArticleOpenUI.Models.Article
{
    public class ArticleModel : IArticle
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public string MouldFile { get; set; } = "";
        public ArticleType Type { get; set; }
        public string Url { get; set; } = "";
        public string Cad { get; set; } = "";
        public string Customer { get; set; } = "";
        public string Description { get; set; } = "";
        public string Material { get; set; } = "";
        public string Shrinkage { get; set; } = "";
        public string Machine { get; set; } = "";
        public bool IsModOrVariant { get; set; } = false;
        public List<string> MouldFilePaths { get; set; } = new List<string>();
        public List<string>? Children { get; set; }

        // TODO: Replace constructor with ParseInfo function to remove dependency.
        // TODO: Create Update() function to update singular articles on demand.
        public ArticleModel(ArticleInfoModel info)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));

            Name = info.Name;
            Type = info.Type;
            IsModOrVariant = info.IsModOrVariant;
            Url = info.Url;
            Path = GeneratePath();

            Cad = Type == ArticleType.Tool ? info.CAD : "";
            Customer = info.Customer;
            Description = Type == ArticleType.Plastic ? info.Description : "";
            Material = Type == ArticleType.Plastic ? info.Material : "";
            if (info.Shrinkage.ContainsKey(Name))
                Shrinkage = info.Shrinkage[Name];
            Machine = info.Machine;
            GetMouldPaths();

            // TODO: Replace with getter that caches after first use
            if (info.Plastics is not null && info.Plastics.Any())
                Children = info.Plastics;
            else
                Children = null;
        }

        public void GetMouldPaths()
        {
            if (Type == ArticleType.Plastic)
                return;

            MouldFilePaths.Clear();
            MouldFile = "";

            var path = Path;
            if (IsModOrVariant)
                path += @$"\{Name}";
            var files = Directory.GetFiles(@$"{path}\CAD");
            var mldFiles = files.Where(x => x.EndsWith(".mld"));
            MouldFilePaths = mldFiles.ToList();
        }

        // TODO: Add OpenMouldReadOnly (parameter flag?)
        public void OpenMould()
        {
            var app = new TopSolid.Application();
            if (File.Exists(MouldFile))
#if DEBUG
                app.Documents.Load(MouldFile, ReadOnly: true);
#else
				app.Documents.Load(MouldFile);
#endif
        }

        // TODO: Replace with getter
        private string GeneratePath()
        {
            var basePath = $@"\\server1\ArtikelFiler\ArticleFiles\{Name}";
            if (IsModOrVariant)
            {
                if (Type == ArticleType.Tool)
                    basePath = basePath[0..^1];
                else if (Type == ArticleType.Plastic)
                    basePath = basePath[0..^2];
            }

            var fullPath = basePath + @"\" + Name;

            if (IsModOrVariant)
                return basePath;

            return fullPath;
        }
    }
}
