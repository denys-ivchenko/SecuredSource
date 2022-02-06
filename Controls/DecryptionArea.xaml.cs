using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Threading.Tasks;

using Telesyk;

using Telesyk.SecuredSource.Globalization;

using oper = Telesyk.SecuredSource.ApplicationOperator;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class DecryptionAreaControl : UserControl
	{
		#region Private declarations

		private TickTimer _timer;

		#endregion

		#region Constructors

		public DecryptionAreaControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public IMainWindow Host { get; set; }

		public DecryptionPanelControl Panel => ControlPanel;

		#endregion

		#region Public methods

		public void SetPanelWidth(int width) => setPanelWidth(width);

		#endregion

		#region Protected properties

		protected int lastPercentage;

		#endregion

		#region Private methods

		private void init()
		{
			oper.Operator.DeserializePackChanged += (s, a) => checkStartButton();

			ApplicationSettings.Current.DecryptionAlgorithmChanged += (s, a) => checkStartButton();

			ApplicationSettings.Current.DecryptionDirectoryChanged += (s, a) => checkStartButton();

			ApplicationSettings.Current.DecryptionPasswordChanged += (s, a) => checkStartButton();

			ColumnPanel.Width = new GridLength(ApplicationSettings.Current.PanelWidth);

			checkStartButton();
		}

		private void progress(int percentage)
		{
			if (percentage > lastPercentage)
			{
				lastPercentage = percentage;

				Debug.WriteLine($"percentage: {ControlProgress.Value = percentage}");
			}
		}

		private void checkStartButton()
		{
			ButtonStart.IsEnabled = checkPackFilePathAndDirectory();
		}

		private bool checkPackFilePathAndDirectory()
		{
			if (string.IsNullOrEmpty(ApplicationSettings.Current.DecryptionPackFilePath) || string.IsNullOrEmpty(ApplicationSettings.Current.DecryptionDirectory))
				return false;

			var file = new FileInfo(ApplicationSettings.Current.DecryptionPackFilePath ?? @"c:\not-exist-directoty-$$\not-exist-file.$$");
			var directory = new DirectoryInfo(ApplicationSettings.Current.DecryptionDirectory ?? @"c:\not-exist-directoty-$$");

			if (!file.Exists || !directory.Exists)
				return false;

			if (oper.Operator.DeserializePack == null || !oper.Operator.DeserializePack.IsValid)
				return false;

			if (ApplicationSettings.Current.DecryptionAlgorithm != oper.Operator.DeserializePack.FilePack.Algorithm)
				return false;

			if (oper.Operator.DeserializePack.FilePack.Password.Hash != ApplicationSettings.Current.DecryptionPassword?.Hash)
				return false;

			return true;
		}

		private void saveSplitterWidth()
		{
			ApplicationSettings.Current.PanelWidth = (int)ColumnPanel.ActualWidth;

			Host.EncryptionArea.SetPanelWidth(ApplicationSettings.Current.PanelWidth);
		}

		private void setPanelWidth(int width)
		{
			ColumnPanel.Width = new GridLength(width);
		}

		private void start()
		{
			oper.Operator.DeserializePack = new EncryptionPack(ApplicationSettings.Current.DecryptionPackFilePath, ApplicationSettings.Current.DecryptionDirectory);

			oper.Operator.DeserializePack.Processed += (s, a) => progress(a.Value);
			oper.Operator.DeserializePack.Finished += (s, a) => finished(100);

			ControlProgress.Value = 0;
			ControlProgress.Visibility = Visibility.Visible;
			TextTime.Visibility = Visibility.Visible;

			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = false);

			_timer = new TickTimer(1000, writeTime);
			_timer.Start();

			oper.Operator.DeserializePack.Deserialize();
		}

		private void writeTime(TickTimer timer)
		{
			TextTime.Text = timer.LastTickEllapsed.ToString(@"hh\:mm\:ss");
		}

		private void finished(int percentage)
		{
			lastPercentage = 0;

			ControlProgress.Value = percentage;
			ControlProgress.Visibility = Visibility.Hidden;

			TextTime.Visibility = Visibility.Collapsed;

			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = true);

			_timer.Stop(true);

			if (oper.Operator.DeserializePack.IsFaulted)
				MessageBox.Show(oper.Operator.DeserializePack.Error.Message, oper.Operator.DeserializePack.Error.GetType().Name);
		}

		private void stop() => oper.Operator.DeserializePack.Cancel();

		#region Handlers

		private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e) => saveSplitterWidth();

		private void Serialize_Finished(object sender, EventArgs e) => finished(100);

		private void Serialize_Processed(object sender, ValueProcessedEventArgs<int> args) => progress(args.Value);

		private void ButtonStop_Click(object sender, RoutedEventArgs e) => stop();

		private void ButtonStart_Click(object sender, RoutedEventArgs e) => start();

		//private void ButtonRead_Click(object sender, RoutedEventArgs e) => readPack();

		#endregion

		#endregion
	}
}
