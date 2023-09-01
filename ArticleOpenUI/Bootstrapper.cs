using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using ArticleOpenUI.ViewModels;

namespace ArticleOpenUI
{
	public class Bootstrapper : BootstrapperBase
	{
		private readonly SimpleContainer m_Container = new SimpleContainer();
		public Bootstrapper()
		{
			Initialize();
		}
		protected override void Configure()
		{
			m_Container.Instance(m_Container);

			m_Container
				.Singleton<IEventAggregator, EventAggregator>()
				.Singleton<IWindowManager, WindowManager>();

			GetType().Assembly.GetTypes()
				.Where(type => type.IsClass)
				.Where(type => type.Name.EndsWith("ViewModel"))
				.ToList()
				.ForEach(viewModelType => m_Container.RegisterPerRequest(
					viewModelType, viewModelType.ToString(), viewModelType));
		}
		protected override object GetInstance(Type serviceType, string key)
		{
			return m_Container.GetInstance(serviceType, key);
		}
		protected override IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return m_Container.GetAllInstances(serviceType);
		}
		protected override void BuildUp(object instance)
		{
			m_Container.BuildUp(instance);
		}
		protected override void OnStartup(object sender, StartupEventArgs e)
		{

			DisplayRootViewForAsync<ShellViewModel>();
		}

	}
}
