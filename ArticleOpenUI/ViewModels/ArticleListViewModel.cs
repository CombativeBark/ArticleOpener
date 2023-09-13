using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Caliburn.Micro;
using ArticleOpenUI.Models;

namespace ArticleOpenUI.ViewModels
{
	internal class ArticleListViewModel : Screen, IListTabItem
	{
		private static readonly Regex reName = new Regex(@"\[\d+\] (.*)");
		private readonly IWindowManager m_WindowManager;
		private readonly IEventAggregator m_EventAggregator;

		private string m_TabName = "Tab";

		public string TabName 
		{ 
			get => m_TabName; 
			set 
			{
				m_TabName = value;
				RenameTab(); 
			} 
		}

		public int Count { get => Articles.Count; }
		public ObservableCollection<ArticleModel> Articles { get; private set; } = new ObservableCollection<ArticleModel>();
		public TabItemType Type { get => TabItemType.ArticleList; }

		public bool IsPinned = false;

		public ArticleListViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
		{
			m_WindowManager = windowManager;
			m_EventAggregator = eventAggregator;

			m_TabName = DisplayName;
		}

		public void AddArticle(ArticleModel article)
		{
			if (Articles.Count == 0)
				TabName = article.Customer;
			Articles.Add(article);
			RenameTab();
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
		public void OpenInfoPage(object? listItem)
		{
			if (listItem is not ArticleModel article) 
				return;

			try
			{
				article.OpenInfoPage();
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
			RenameTab();
		}
		public void RenameTab(string? newName = null)
		{
			if (newName is not null)
				m_TabName = newName;

			DisplayName = AddCounter(m_TabName);
		}
		private string AddCounter(string name)
		{
			return string.Format("[{0}] {1}", Articles.Count, name);
		}
	}
}
