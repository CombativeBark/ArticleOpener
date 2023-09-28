using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using ArticleOpenUI.Models.Article;

namespace ArticleOpenUI.ViewModels
{
    class MouldSelectViewModel : Screen
	{
		private readonly Dictionary<string, string> m_MouldHashes;
		private readonly ArticleModel m_ReferencedArticle;

		public string Name { get; init; }
		public ObservableCollection<string> MouldFiles { get; private set; }
		public string SelectedFile { get; set; }

		public MouldSelectViewModel(ArticleModel article)
		{
			m_MouldHashes= new Dictionary<string, string>();
			MouldFiles = new ObservableCollection<string>();
			SelectedFile = string.Empty;

			m_ReferencedArticle = article;

			Name = m_ReferencedArticle.Name;

			PopulateList(m_ReferencedArticle.MouldFilePaths);
		}

		public void SelectFile()
		{
			if (SelectedFile != string.Empty)
				m_ReferencedArticle.MouldFile = m_MouldHashes[SelectedFile];
			TryCloseAsync();
		}
		
		private void PopulateList(List<string> files)
		{
			files.ForEach(file => {
				var filename = FormatFilename(file);
				m_MouldHashes.Add(filename, file);
				MouldFiles.Add(filename);
				});
		}
		private string FormatFilename(string filename)
		{
			return filename.Split("\\").Last();
		}
	}
}
