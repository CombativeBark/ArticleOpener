using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleOpenUI.Models.Article
{
	public interface ITool : IArticle
	{
		string CAD { get; set; }
		void OpenMould();
	}
}
