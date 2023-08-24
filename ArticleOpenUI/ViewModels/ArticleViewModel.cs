using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArticleOpenUI.ViewModels
{
	class ArticleViewModel : Conductor<ArticleListViewModel>.Collection.OneActive
	{
		private IEventAggregator m_EventAggregator;
		private IWindowManager m_WindowManager;
		private ArticleListViewModel? m_SelectedArticleList;
		private string m_Input;
		private int m_TabCounter;
		private TextBlock? m_RenameTextBlock;
		private TextBox? m_RenameTextBox;

		public ArticleListViewModel SelectedArticleList
		{
			get
			{
				if (m_SelectedArticleList == null)
				{
					CreateNewTab();
					m_SelectedArticleList = Items.First();
				}
				return m_SelectedArticleList;
			}
			set
			{
				m_SelectedArticleList = value;
				NotifyOfPropertyChange(() => SelectedArticleList);
			}
		}
		public string Input
		{
			get => m_Input;
			set 
			{ 
				m_Input = value; 
				NotifyOfPropertyChange(() => Input);
			}
		}
		public ArticleViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
		{
			m_EventAggregator = eventAggregator;
			m_WindowManager = windowManager;

			m_TabCounter = 0;
#if DEBUG
			m_Input = "302981V 304092V";
#else
			m_Input = "";
#endif
		}

		public void TextBoxEvent(ActionExecutionContext context)
		{
			var keyArgs = context.EventArgs as KeyEventArgs;

			if (keyArgs == null || string.IsNullOrWhiteSpace(Input))
				return;

			switch (keyArgs.Key)
			{
				case Key.Enter:
					SearchArticle();
					break;
				case Key.Escape:
					ClearQueue();
					break;
				default:
					break;
			}
		}
		public void SearchArticle()
		{
			if (Input == null || string.IsNullOrEmpty(Input))
				return;

			foreach (var articleNumber in SplitString(Input))
			{
				try
				{
					var newArticle = ArticleFactory.CreateArticle(articleNumber);

					AddToQueue(newArticle);
					if (newArticle.Children != null && newArticle.Children.Any())
					{
						newArticle.Children.ForEach(x =>
						{
							try
							{
								var newPlasticArticle = ArticleFactory.CreateArticle(x);
								AddToQueue(newPlasticArticle);
							}
							catch (Exception e)
							{
								MessageBox.Show("Error: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							}
						});
					}
				}
				catch (Exception e)
				{
#if (DEBUG)
					MessageBox.Show($"Error: {e.Message}\n\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#else
					MessageBox.Show("Error: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

				}
			}
		}
		// TODO: Double-click to clear input
		public void ClearQueue()
		{
			SelectedArticleList.Articles.Clear();
		}
		private void AddToQueue(ArticleModel article)
		{
			if (!IsInQueue(article.Name))
			{
				SelectedArticleList.Articles.Add(article);
			}
		}
		private bool IsInQueue(string inputArticle)
		{
			foreach (var article in SelectedArticleList.Articles)
			{
				if (article.Name.Equals(inputArticle))
					return true;
			}
			return false;
		}
		private List<string> SplitString(string input)
		{
			List<string> result = new();

			var splitString = input.Split(new char[] { ' ', '.', ':', ',', ';', '-', '_' });
			for (int i = 0; i < splitString.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(splitString[i]))
				{
					result.Add(splitString[i]);
				}
			}
			return result;
		}
		public void PinTab(object dataContext, object source)
		{
			var context = dataContext as ArticleListViewModel;
			var sourceContext = source as MenuItem;
			if (context == null || sourceContext == null) 
				return;

			context.IsPinned = !context.IsPinned;
			if (context.IsPinned)
				sourceContext.Header = "Unpin";
			else
				sourceContext.Header = "Pin";
		}
		public void CreateNewTab()
		{
			var newTab = new ArticleListViewModel(m_WindowManager, m_EventAggregator);

			if (Items.Count == 0)
				m_TabCounter = 0;
			newTab.DisplayName = $"Tab {++m_TabCounter}";

			Items.Add(newTab);
			SelectedArticleList = newTab;
		}
		public void CloseTab(object dataContext)
		{
			var context = dataContext as ArticleListViewModel;
			if (context == null)
				return;
			if (context.IsPinned)
			{
				var result = MessageBox.Show($"Are you sure you would like to close '{context.DisplayName}'",
											 $"Close {context.DisplayName}", 
											 MessageBoxButton.YesNo, 
											 MessageBoxImage.Question);
				if (result == MessageBoxResult.No)
					return;
			}

			Items.Remove(context);
			if (Items.Count == 0) 
				CreateNewTab();
		}
		public void RenameTab(object source)
		{
			var context = source as FrameworkElement;
			if (context == null) 
				return;

			var contextMenu = (ContextMenu)context.Parent;
			var stackPanel = contextMenu?.PlacementTarget as StackPanel;
			if (contextMenu == null || stackPanel == null)
				return;

			m_RenameTextBlock = (TextBlock)stackPanel.Children[0];
			m_RenameTextBox = (TextBox)stackPanel.Children[1];
			if (m_RenameTextBlock == null || m_RenameTextBox == null)
				throw new ArgumentNullException();

			m_RenameTextBlock.Visibility = Visibility.Collapsed;
			m_RenameTextBox.Visibility = Visibility.Visible;
			m_RenameTextBox.Focus();
			m_RenameTextBox.SelectAll();
		}
		public void RenameTabFinalize(ActionExecutionContext executionContext)
		{
			var keyArgs = executionContext.EventArgs as KeyEventArgs;
			if (keyArgs == null)
				return;
			if (keyArgs.Key != Key.Enter && 
				keyArgs.Key != Key.Escape)
				return;

			if (m_RenameTextBlock == null || m_RenameTextBox == null)
				throw new ArgumentNullException();
			m_RenameTextBlock.Visibility = Visibility.Visible;
			m_RenameTextBox.Visibility = Visibility.Collapsed;
		}
	}
}
