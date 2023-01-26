﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ArticleOpenUI.Models
{
	public abstract class ArticleBase
	{
		public abstract string Name { get; }
		public abstract string Path { get; }
		public abstract ArticleType Type { get; }
		public virtual string Url { get; } = "";
		public virtual string Cad { get; } = "";
		public virtual string Customer { get; } = "";
		public virtual string Description { get; } = "";
		public virtual string Material { get; } = "";
		public virtual string Shrinkage { get; } = "";
		public virtual string Machine { get; } = "";
		public abstract List<string>? Children { get; }

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
	}
}
