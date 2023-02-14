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
		public string Path { get; init; } = "";
		public ArticleType Type { get; init; }
		public string Url { get; init; } = "";
		public string Cad { get; init; } = "";
		public string Customer { get; init; } = "";
		public string Description { get; init; } = "";
		public string Material { get; init; } = "";
		public string Shrinkage { get; init; } = "";
		public string Machine { get; init; } = "";
		public bool IsModOrVariant { get; private set; } = false;
		public List<string>? Children { get; init; }

		public ArticleModel(ArticleInfo info)
		{
			if (info is null)
				throw new ArgumentNullException(nameof(info));

			Name = info.Name;
			Type = info.Type;
			Url = info.Url;
			Path = GeneratePath();

			Cad = Type == ArticleType.Tool ? info.CAD : "";
			Customer = info.Customer;
			Description	= Type == ArticleType.Plastic ? info.Description : "";
			Material = Type == ArticleType.Plastic ? info.Material : "";
			Shrinkage = info.Shrinkage;
			Machine = info.Machine;

			if (info.Plastics is not null && info.Plastics.Any())
				Children = info.Plastics;
			else 
				Children = null;
		}

		public void OpenFolder()
		{
			if (Directory.Exists(Path))
			{
				ProcessStartInfo startInfo = new ProcessStartInfo()
				{
					Arguments = Path,
					FileName = "explorer.exe"
				};

				Process.Start(startInfo);
			}
			else
			{
				throw new Exception($"Error: Directory for Article {Name} doesn't exist");
			}
		}
		public void OpenInfo()
		{
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
					basePath = basePath.Substring(0, basePath.Length - 1);
				else if ( Type == ArticleType.Plastic)
					basePath = basePath.Substring(0, basePath.Length - 2);
			}

			var fullPath = basePath + @"\" + Name;

			if (!Directory.Exists(fullPath))
				throw new DirectoryNotFoundException($"Directory for {Name} doesn't exist.");

			if (IsModOrVariant)
				return basePath;

			return fullPath;
		}
	}
}
