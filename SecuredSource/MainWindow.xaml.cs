using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Telesyk.SecuredSource.Globalization;
using Telesyk.SecuredSource.UI.Controls;

using oper = Telesyk.SecuredSource.ApplicationOperator;

namespace Telesyk.SecuredSource.UI
{
	public partial class MainWindow : Window, IMainWindow
	{
		#region Private declarations

		private ApplicationMode _mode = ApplicationMode.Decryption;
		private bool _skipModeChangingReaction = false;

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
			private set { setMode(value); }
		}

		public DecryptionPanelControl DecryptionPanel { get; private set; }

		public DecryptionAreaControl DecryptionArea { get; private set; } = new DecryptionAreaControl();

		public EncryptionPanelControl EncryptionPanel { get; private set; }

		public EncryptionAreaControl EncryptionArea { get; private set; } = new EncryptionAreaControl();

		#endregion

		#region Private methods

		private void init()
		{
			ApplicationSettings.Current.AppVersion = $"{Assembly.GetExecutingAssembly().GetName().Version}";

			oper.Operator.InitStartUpFile();

			Mode = ApplicationSettings.Current.Mode;

			DecryptionPanel = DecryptionArea.Panel;
			EncryptionPanel = EncryptionArea.Panel;

			DecryptionArea.Host = this;
			EncryptionArea.Host = this;

			var textVersion = ((Run)FindName("textVersion"));

			textVersion.ToolTip = textVersion.Text = ApplicationSettings.Current.AppVersion;
			
			Width = ApplicationSettings.Current.WindowWidth;
			Height = ApplicationSettings.Current.WindowHeight;
			Top = ApplicationSettings.Current.WindowTop;
			Left = ApplicationSettings.Current.WindowLeft;

			oper.Operator.RegisterForEncryptionProcess(RadioEncryption, RadioDecryption);

			initLanguage();
		}

		private void initLanguage()
		{
			var lang = Thread.CurrentThread.CurrentUICulture.ThreeLetterWindowsLanguageName.ToUpper();
			var index = 1;

			if (lang == "UKR")
				index = 0;
			else if (lang == "RUS")
				index = 2;

			SelectLanguage.SelectedIndex = index;

			SelectLanguage.SelectionChanged += (s, a) =>
			{
				var name = "en-US";

				if (SelectLanguage.SelectedIndex == 0)
					name = "uk-UA";
				else if (SelectLanguage.SelectedIndex == 2)
					name = "ru-RU";

				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(name);

				new MainWindow().Show();

				Close();
			};
		}

		private void setMode(ApplicationMode mode)
		{
			_mode = mode;

			ApplicationSettings.Current.Mode = Mode;
			
			if (Mode == ApplicationMode.Decryption && !(RadioDecryption.IsChecked ?? false))
				RadioDecryption.IsChecked = true;

			if (Mode == ApplicationMode.Encryption && !(RadioEncryption.IsChecked ?? false))
				RadioEncryption.IsChecked = true;

			ControlMainArea.Children.Clear();
			ControlMainArea.Children.Add(mode == ApplicationMode.Decryption ? (UserControl)DecryptionArea : EncryptionArea);

			if (mode == ApplicationMode.Decryption)
				oper.Operator.LoadDeserializePack(ApplicationSettings.Current.DecryptionPackFilePath);
		}

		#region Handlers

		private void radioMode_Checked(object sender, RoutedEventArgs e)
		{
			Enum.TryParse<ApplicationMode>(((RadioButton)e.Source).Name.Substring(5), out ApplicationMode mode);

			if (Mode != mode)
				Mode = mode;
		}

		private void TextBlock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var datas = Strings.InfoDialog.Split('|');

			string text = string.Empty;

			foreach (var data in datas)
				text += $"{data}{Environment.NewLine}";
			
			text += Environment.NewLine + Environment.NewLine + Strings.Enigma;
			text += Environment.NewLine + Environment.NewLine + Strings.EnigmaLinkText;

			var result = MessageBox.Show(text, Strings.Title, MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes)
				Process.Start(Strings.EnigmaLink);
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
