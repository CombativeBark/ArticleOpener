using ArticleOpenUI.Events;
using ArticleOpenUI.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArticleOpenUI.ViewModels
{
	class ArticleViewModel : Conductor<IListTabItem>.Collection.OneActive, IHandle<NewTabEvent>
	{
		private readonly SimpleContainer m_Container;
		private readonly IEventAggregator m_EventAggregator;
		private readonly IWindowManager m_WindowManager;
		private string m_Input;
		private int m_TabCounter;
		private TextBlock? m_RenameTextBlock;
		private TextBox? m_RenameTextBox;

		public string Input
		{
			get => m_Input;
			set 
			{ 
				m_Input = value; 
				NotifyOfPropertyChange(() => Input);
			}
		}
		public ArticleViewModel(SimpleContainer container, IEventAggregator eventAggregator, IWindowManager windowManager)
		{
			m_Container = container;
			m_WindowManager = windowManager;
			m_EventAggregator = eventAggregator;

			m_EventAggregator.SubscribeOnUIThread(this);


			m_TabCounter = 0;
#if DEBUG
			m_Input = "302981V 304092V";
#else
			m_Input = "";
#endif

			InitializeTabs();
		}

		public void TextBoxEvent(ActionExecutionContext context)
		{
			if (context.EventArgs is not KeyEventArgs keyArgs || 
				string.IsNullOrWhiteSpace(Input))
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
		public void ClearInput()
		{
			Input = string.Empty;
		}
		public void SearchArticle()
		{
			if (Input == null || 
				string.IsNullOrEmpty(Input))
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
			ActiveItem.Articles.Clear();
		}
		private void AddToQueue(ArticleModel article)
		{
			if (!IsInQueue(article.Name))
			{
				ActiveItem.AddArticle(article);
			}
		}
		private bool IsInQueue(string inputArticle)
		{
			foreach (var article in ActiveItem.Articles)
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
		private void InitializeTabs()
		{
			CreateNewTabButton();
			CreateNewTab();
		}
		public void PinTab(object dataContext, object source)
		{
			if (dataContext is not ArticleListViewModel context || 
				source is not MenuItem sourceContext)
				return;

			var contextMenu = (ContextMenu)sourceContext.Parent;
			if (contextMenu == null || 
				contextMenu?.PlacementTarget is not StackPanel stackPanel)
				return;

			// Silent error potential yay.
			if (stackPanel.Children[0] is not TextBlock pinIcon)
				throw new NullReferenceException($"Couldn't find pin icon for {context.DisplayName}");

			context.IsPinned = !context.IsPinned;
			if (context.IsPinned)
			{
				sourceContext.Header = "Unpin";
				pinIcon.Visibility = Visibility.Visible;
			}
			else
			{
				sourceContext.Header = "Pin";
				pinIcon.Visibility = Visibility.Collapsed;
			}
		}
		public void CreateNewTab()
		{
			var newTab = m_Container.GetInstance<ArticleListViewModel>();

			if (Items.Count == 1)
			{
				m_TabCounter = 0;
			}
			newTab.NewName = string.Format("Tab {0}", ++m_TabCounter);

			Items.Insert(Items.Count - 1,newTab);
			ActiveItem = newTab;
		}

		private void CreateNewTabButton()
		{
			Items.Add(m_Container.GetInstance<NewTabListViewModel>());
		}
		public string CanCloseTab(object dataContext)
		{
			if (dataContext as IListTabItem is null or NewTabListViewModel)
				return "Collapsed";
			return "Visible";
		}
		public void CloseTab(object dataContext)
		{
			if (dataContext is not ArticleListViewModel context)
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

			if (Items.Count >= 3)
				ActivateItemAsync(Items[Items.Count() - 3]);

			Items.Remove(context);
		}
		public void RenameTab(object source)
		{
			if (source is not FrameworkElement context)
				return;

			if (context.Parent is not ContextMenu contextMenu || 
				contextMenu?.PlacementTarget is not StackPanel stackPanel)
				return;

			// Most likely the cause of a silent error in the future.
			// Life is pain!
			m_RenameTextBlock = stackPanel.Children[1] as TextBlock;
			m_RenameTextBox = stackPanel.Children[2] as TextBox;
			if (m_RenameTextBlock == null || m_RenameTextBox == null)
				throw new ArgumentNullException(nameof(source));

			m_RenameTextBlock.Visibility = Visibility.Collapsed;
			m_RenameTextBox.Visibility = Visibility.Visible;
			m_RenameTextBox.Focus();
			m_RenameTextBox.SelectAll();
		}
		public void RenameTabFinalize(ActionExecutionContext executionContext)
		{
			if (executionContext.EventArgs is not KeyEventArgs keyArgs)
				return;
			if (keyArgs.Key != Key.Enter && 
				keyArgs.Key != Key.Escape)
				return;

			if (m_RenameTextBlock == null || m_RenameTextBox == null)
				throw new ArgumentNullException(nameof(executionContext));
			m_RenameTextBlock.Visibility = Visibility.Visible;
			m_RenameTextBox.Visibility = Visibility.Collapsed;
		}

		public Task HandleAsync(NewTabEvent message, CancellationToken cancellationToken)
		{
			CreateNewTab();
			return Task.CompletedTask;
		}
	}
}
