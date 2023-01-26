using ArticleOpenUI.ViewModels;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ArticleOpenUI
{
	public class Bootstrapper : BootstrapperBase
	{
		public Bootstrapper()
		{
			Initialize();
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			DisplayRootViewForAsync<ArticleViewModel>();
		}

	}
}
