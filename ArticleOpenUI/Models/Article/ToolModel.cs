using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleOpenUI.Models.Article
{
	public class ToolModel : ITool
	{
		public string CAD { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public string Path { get; set; }
		public string Customer { get; set; }
		public string Description { get; set; }
		public string Machine { get; set; }
		public string Material { get; set; }
		public string Shrinkage { get; set; }
		public bool IsModOrVariant { get; set; }

		public ToolModel(string name)
		{
			Name = name;
		}

        public void OpenFolder()
		{
		}

		public void OpenInfoPage()
		{
		}

		public void OpenMould()
		{
		}
	}
}
