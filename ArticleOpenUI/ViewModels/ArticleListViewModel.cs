using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ArticleOpenUI.ViewModels
{
	internal class ArticleListViewModel : Screen, INotifyPropertyChanged
	{
		private readonly IWindowManager m_WindowManager;
		private readonly IEventAggregator m_EventAggregator;

		public ObservableCollection<ArticleModel> Articles { get; private set; } = new ObservableCollection<ArticleModel>();
		public bool IsPinned = false;

		public ArticleListViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
		{
			m_WindowManager = windowManager;
			m_EventAggregator = eventAggregator;
			DisplayName = "New Tab";
		}

		public void AddArticle(ArticleModel article)
		{
			Articles.Add(article);
			NotifyOfPropertyChange(() => Articles);
		}

		public bool CanOpenMould(object context)
		{
			if (context is not ArticleModel item)
				return false;
			if (item.Type == ArticleType.Plastic)
				return false;
			if (!item.MouldFilePaths.Any())
				return false;
			return true;
		}
		public async void OpenMould(object? context)
		{
			if (context is not ArticleModel article)
				return;

			article.GetMouldPaths();
			if (article.MouldFilePaths.Count > 1)
				await m_WindowManager.ShowDialogAsync(new MouldSelectViewModel(article));
			else
				article.MouldFile = article.MouldFilePaths.First();

			if (string.IsNullOrEmpty(article.MouldFile))
				return;
			try
			{
				article.OpenMould();
			}
			catch (Exception e)
			{
				MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		public void OpenFolder(object? context)
		{
			if (context is not ArticleModel article) 
				return;

			try
			{
				article.OpenFolder();
			}
			catch (Exception e)
			{
				MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		public void OpenInfo(object? context)
		{
			if (context is not ArticleModel article) 
				return;

			try
			{
				article.OpenInfo();
			}
			catch (Exception e)
			{
				MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		public void RemoveFromQueue(object? context)
		{
			if (context is not ArticleModel item)
				return;

			Articles.Remove(item);
		}

	}
}
