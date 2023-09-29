using ArticleOpenUI.Models.InfoScraper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleOpenUI.Models.Article
{
	public class ToolModel : ArticleBase, ITool
	{

		public string CAD { get; set; } = "";
		public override string Name { get; set; } = "";
		public override string Url { get; set; } = "";
		public override string Path { get; set; } = "";
		public List<string> MouldFilePaths { get; set; } = new List<string>();
		public override string Customer { get; set; } = "";
		public override string Machine { get; set; } = "";
		public override string Shrinkage { get; set; } = "";
		public override bool IsSpecial { get; set; } = false;
		public int SelectedMouldFileIndex { get; set; } = -1;


		public void OpenMould()
		{
		}

		public void UpdateMouldPaths()
		{
			var path = Path;
			if (string.IsNullOrEmpty(path))
				throw new ArgumentException($"Can't get .MLD filepaths for {path}");

            if (IsSpecial)
                path += @$"\{Name}";

            var filesInFolder = Directory.GetFiles(@$"{path}\CAD");
            var mouldFiles = filesInFolder.Where(x => x.EndsWith(".mld"));

			MouldFilePaths = mouldFiles.ToList();
		}
	}
}
