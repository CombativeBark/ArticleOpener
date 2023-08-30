using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ArticleOpenUI.ViewModels
{
	internal class ArticleListViewModel : Screen, IListTabItem
	{
		private static readonly Regex reName = new Regex(@"\[\d+\] (.*)");
		private readonly IWindowManager m_WindowManager;
		private readonly IEventAggregator m_EventAggregator;
		private int m_Count;
		public int Count { get => m_Count; }
		public ObservableCollection<ArticleModel> Articles { get; private set; } = new ObservableCollection<ArticleModel>();
		public string NewName 
		{ 
			get => DisplayName; 
			set 
			{
				UpdateName(value);
				NotifyOfPropertyChange(() => NewName);
			}
		}
		public TabItemType Type { get => TabItemType.ArticleList; }

		public bool IsPinned = false;

		public ArticleListViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
		{
			m_WindowManager = windowManager;
			m_EventAggregator = eventAggregator;
			m_Count = 0;
		}

		public void AddArticle(ArticleModel article)
		{
			Articles.Add(article);
			m_Count++;
			UpdateName();
			NotifyOfPropertyChange(() => Articles);
		}

		public bool CanOpenMould(object listItem)
		{
			if (listItem is not ArticleModel article)
				return false;
			if (article.Type == ArticleType.Plastic)
				return false;
			if (!article.MouldFilePaths.Any())
				return false;
			return true;
		}
		public async void OpenMould(object? listItem)
		{
			if (listItem is not ArticleModel article)
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
		public void OpenFolder(object? listItem)
		{
			if (listItem is not ArticleModel article) 
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
		public void OpenInfo(object? listItem)
		{
			if (listItem is not ArticleModel article) 
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
		public void RemoveFromQueue(object? listItem)
		{
			if (listItem is not ArticleModel article)
				return;

			Articles.Remove(article);
			m_Count--;
			UpdateName();
		}
		private void UpdateName()
		{
			var oldName = DisplayName;
			var reResult = reName.Match(oldName);
			if (!reResult.Success)
				throw new Exception(string.Format("Unexpected tab name '{0}'", oldName));

			DisplayName = string.Format("[{0}] {1}", Count, reResult.Groups[1].Value);
		}

		private void UpdateName(string newName)
		{
			DisplayName = string.Format("[{0}] {1}", Count, newName);
		}
	}
}
