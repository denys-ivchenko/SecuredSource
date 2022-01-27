using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Threading.Tasks;

using Telesyk;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class DecryptionAreaControl : UserControl
	{
		#region Private declarations

		private IProgress<int> _progress = null;
		private IProgress<int> _finish = null;
		private EncryptionPack _encryptor;
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

		public EncryptionPack Pack { get; private set; }

		public PackData FilePack { get => ControlFiles.FilePack; }

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
			ApplicationSettings.Current.DecryptionPackPathChanged += (s, a) => packChanged();
			ApplicationSettings.Current.DecryptionDirectoryChanged += (s, a) => packDirectoryChanged();

			Panel.PasswordChanged += (s, a) => checkStartButton();

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
			var filePathAndDirectoryIsValid = checkPackFilePathAndDirectory();

			ButtonStart.IsEnabled = filePathAndDirectoryIsValid && Pack != null && Pack.PasswordHash == Panel.PasswordHash;
		}

		private void packChanged()
		{
			if (!string.IsNullOrEmpty(ApplicationSettings.Current.DecryptionPackPath))
			{
				var file = new FileInfo(ApplicationSettings.Current.DecryptionPackPath);

				if (file.Exists)
					Pack = new EncryptionPack(ApplicationSettings.Current.DecryptionPackPath, Panel.Password, ApplicationSettings.Current.DecryptionDirectory);
				else
					Pack = null;
			}

			checkStartButton();
		}

		private void packDirectoryChanged() => checkStartButton();

		private bool checkPackFilePathAndDirectory()
		{
			if (string.IsNullOrEmpty(ApplicationSettings.Current.DecryptionPackPath) || string.IsNullOrEmpty(ApplicationSettings.Current.DecryptionDirectory))
				return false;

			var file = new FileInfo(ApplicationSettings.Current.DecryptionPackPath ?? @"c:\not-exist-directoty-$$\not-exist-file.$$");
			var directory = new DirectoryInfo(ApplicationSettings.Current.DecryptionDirectory ?? @"c:\not-exist-directoty-$$");

			return file.Exists && directory.Exists;
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
			var pack = new EncryptionPack(ApplicationSettings.Current.DecryptionPackPath, Panel.Password, ApplicationSettings.Current.DecryptionDirectory);

			pack.Finished += (s, a) =>
			{
				
			};

			pack.Deserialize();
		}

		private void started()
		{
			_timer = new TickTimer(1000, writeTime);
			_timer.Start();
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
			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = true);

			_timer.Stop();

			if (_encryptor.IsFaulted)
				for (var i = 0; i < _encryptor.Exception.InnerExceptions.Count; i++)
					MessageBox.Show(_encryptor.Exception.InnerExceptions[i].Message, _encryptor.Exception.InnerExceptions[i].GetType().Name);
		}

		private void stop() => _encryptor.Cancel();

		#region Handlers

		private void ApplicationSettings_PasswordChanged(object sender, EventArgs e) => checkStartButton();

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
