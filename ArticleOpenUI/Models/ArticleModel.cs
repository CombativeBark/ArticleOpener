using ArticleOpenUI.ViewModels;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ArticleOpenUI.Models
{
	public class ArticleModel
	{
		public string Name { get; init; } = "";
		public string Path { get; set; } = "";
		public string MouldFile { get; set; } = "";
		public ArticleType Type { get; init; }
		public string Url { get; init; } = "";
		public string Cad { get; init; } = "";
		public string Customer { get; init; } = "";
		public string Description { get; init; } = "";
		public string Material { get; init; } = "";
		public string Shrinkage { get; init; } = "";
		public string Machine { get; init; } = "";
		public bool IsModOrVariant { get; private set; } = false;
		public List<string> MouldFilePaths { get; set; } = new List<string>();
		public List<ArticleInfo>? Children { get; init; }

		public ArticleModel(ArticleInfo info)
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

		public void OpenFolder()
		{
			if (!Directory.Exists(Path))
				throw new DirectoryNotFoundException($"Directory for Article {Name} doesn't exist");

			var startInfo = new ProcessStartInfo()
			{
				Arguments = Path,
				FileName = "explorer.exe"
			};

			Process.Start(startInfo);
		}
		public void OpenInfo()
		{
			if (Url == null || string.IsNullOrWhiteSpace(Url))
				throw new ArgumentNullException(Url, $"URL for Article {Name} is missing.");

			ProcessStartInfo startInfo = new()
			{
				FileName = Url,
				UseShellExecute = true,
			};
			Process.Start(startInfo);
		}

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
