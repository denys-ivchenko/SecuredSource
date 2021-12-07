using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Telesyk.SecuredSource.UI.Controls;

namespace Telesyk.SecuredSource.UI
{
	public partial class MainWindow : Window
	{
		#region Private declarations

		private ApplicationMode _mode = ApplicationMode.SimpleAndFast;

		#endregion

		#region Constructors

		public MainWindow()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public ApplicationMode Mode
		{
			get { return _mode; }
			private set { ensureMode(value); }
		}

		public SimpleAndFastAreaControl SimpleAndFastControl { get; private set; } = new SimpleAndFastAreaControl();

		public MoreFeaturesAreaControl MoreFeaturesControl { get; private set; } = new MoreFeaturesAreaControl();

		#endregion

		#region Private methods

		private void init()
		{
			Mode = ApplicationSettings.Current.LastMode;

			var textVersion = ((Run)FindName("textVersion"));
			textVersion.ToolTip = textVersion.Text = ((AssemblyFileVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0]).Version;
		}

		private void ensureMode(ApplicationMode mode)
		{
			_mode = ApplicationSettings.Current.LastMode = mode;

			switch (Mode)
			{
				case ApplicationMode.SimpleAndFast:
					RadioSimpleAndFast.IsChecked = true;
					break;
				case ApplicationMode.MoreFeatures:
					RadioMoreFeatures.IsChecked = true;
					break;
			}

			ensureAreaControl();
		}

		private void ensureAreaControl()
		{
			UserControl controlArea = null;
			
			switch(Mode)
			{
				case ApplicationMode.SimpleAndFast:
					controlArea = SimpleAndFastControl;
					break;
				case ApplicationMode.MoreFeatures:
					controlArea = MoreFeaturesControl;
					break;
			}

			MainAreaPanel.Children.Clear();
			MainAreaPanel.Children.Add(controlArea);
		}

		#region Handlers

		private void radioMode_Checked(object sender, RoutedEventArgs e)
		{
			Enum.TryParse<ApplicationMode>(((RadioButton)e.Source).Name.Substring(5), out ApplicationMode mode);

			Mode = mode;
		}

		#endregion

		#endregion
	}
}
