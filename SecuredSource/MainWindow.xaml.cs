using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Telesyk.SecuredSource.Globalization;
using Telesyk.SecuredSource.UI.Controls;

namespace Telesyk.SecuredSource.UI
{
	public partial class MainWindow : Window, IMainWindow
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

		public SimpleAndFastPanelControl SimpleAndFastPanel { get; private set; }

		public SimpleAndFastAreaControl SimpleAndFastArea { get; private set; } = new SimpleAndFastAreaControl();

		public MoreFeaturesPanelControl MoreFeaturesPanel { get; private set; }

		public MoreFeaturesAreaControl MoreFeaturesArea { get; private set; } = new MoreFeaturesAreaControl();

		#endregion

		#region Private methods

		private void init()
		{
			Mode = ApplicationSettings.Current.Mode;

			SimpleAndFastPanel = SimpleAndFastArea.Panel;
			MoreFeaturesPanel = MoreFeaturesArea.Panel;

			SimpleAndFastArea.Host = this;
			MoreFeaturesArea.Host = this;

			var textVersion = ((Run)FindName("textVersion"));
			textVersion.ToolTip = textVersion.Text = ((AssemblyFileVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0]).Version;

			Width = ApplicationSettings.Current.WindowWidth;
			Height = ApplicationSettings.Current.WindowHeight;
			Top = ApplicationSettings.Current.WindowTop;
			Left = ApplicationSettings.Current.WindowLeft;

			ControlStateOperator.Operator.RegisterForEncryptionProcess(RadioMoreFeatures, RadioSimpleAndFast);
		}

		private void ensureMode(ApplicationMode mode)
		{
			_mode = ApplicationSettings.Current.Mode = mode;

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
					controlArea = SimpleAndFastArea;
					break;
				case ApplicationMode.MoreFeatures:
					controlArea = MoreFeaturesArea;
					break;
			}

			ControlMainArea.Children.Clear();
			ControlMainArea.Children.Add(controlArea);
		}

		#region Handlers

		private void radioMode_Checked(object sender, RoutedEventArgs e)
		{
			Enum.TryParse<ApplicationMode>(((RadioButton)e.Source).Name.Substring(5), out ApplicationMode mode);

			Mode = mode;
		}

		private void TextBlock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			MessageBox.Show(Strings.InfoDialog);
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ApplicationSettings.Current.WindowWidth = (int)e.NewSize.Width;
			ApplicationSettings.Current.WindowHeight = (int)e.NewSize.Height;
		}

		private void Window_LocationChanged(object sender, EventArgs e)
		{
			ApplicationSettings.Current.WindowTop = (int)Top;
			ApplicationSettings.Current.WindowLeft = (int)Left;
		}

		#endregion

		#endregion
	}
}
