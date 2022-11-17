using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleOpenUI.ViewModels
{
    class ShellViewModel
    {
		private string m_Name = "Person";

		public string Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

	}
}
