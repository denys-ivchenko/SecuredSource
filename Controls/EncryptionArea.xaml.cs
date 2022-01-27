using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class EncryptionAreaControl : UserControl
	{
		#region Private declarations

		private IProgress<int> _progress = null;
		private EncryptionPack _encryptor;
		private TickTimer _timer;

		#endregion

		#region Constructors

		public EncryptionAreaControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public PackData FilePack { get => ControlFiles.FilePack; }

		public IMainWindow Host { get; set; }

		public EncryptionPanelControl Panel => ControlPanel;

		#endregion

		#region Protected properties

		protected int lastPercentage;

		#endregion

		#region Public methods

		public void SetPanelWidth(int width) => setPanelWidth(width);

		#endregion

		#region Overridies

		#endregion

		#region Private methods

		private void init()
		{
			ApplicationSettings.Current.AlgorithmChanged += ApplicationSettings_AlgorithmChanged;
			ApplicationSettings.Current.PasswordChanged += ApplicationSettings_PasswordChanged;
			ApplicationSettings.Current.FileQuantityChanged += ApplicationSettings_FileQuantityChanged;

			//ControlFiles.FilesChanged += ControlFiles_FilesChanged;

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
			ButtonStart.IsEnabled = ApplicationSettings.Current.PasswordSize.IsValid(ApplicationSettings.Current.PasswordLength) && ControlFiles.FilePack.FileCount > 0;
		}

		private void saveSplitterWidth()
		{
			ApplicationSettings.Current.PanelWidth = (int)ColumnPanel.ActualWidth; 
			
			Host.DecryptionArea.SetPanelWidth(ApplicationSettings.Current.PanelWidth);
		}

		private void setPanelWidth(int width)
		{
			ColumnPanel.Width = new GridLength(width);
		}

		private void start()
		{
			EncryptionPack serializer = new EncryptionPack(Path.Combine(ApplicationSettings.Current.Directory, $"{ApplicationSettings.Current.FileName}.$$"), ControlPanel.Password, FilePack);
			serializer.Finished += Serialize_Finished;
			serializer.Processed += Serialize_Processed;

			ControlProgress.Value = 0;
			ControlProgress.Visibility = Visibility.Visible;
			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = false);

			_encryptor = serializer;

			_timer = new TickTimer(1000, writeTime);
			_timer.Start();

			serializer.Serialize();
		}

		private void writeTime(TickTimer timer)
		{
			TextTime.Text = timer.LastTickEllapsed.ToString(@"hh\:mm\:ss");
		}

		private void finished(int percentage)
		{
			_timer.Stop(); 
			
			lastPercentage = 0;

			TextTime.Text = null;

			ControlProgress.Value = percentage;
			ControlProgress.Visibility = Visibility.Hidden;
			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = true);

			if (_encryptor.IsFaulted)
				for (var i = 0; i < _encryptor.Exception.InnerExceptions.Count; i++)
					MessageBox.Show(_encryptor.Exception.InnerExceptions[i].Message, _encryptor.Exception.InnerExceptions[i].GetType().Name);
		}

		private void stop() => _encryptor.Cancel();

		#region Handlers

		private void ApplicationSettings_PasswordChanged(object sender, EventArgs e)
		{
			FilePack.PasswordHash = Panel.PasswordHash;

			checkStartButton();
		}

		private void ApplicationSettings_AlgorithmChanged(object sender, EventArgs e)
		{
			FilePack.Algorithm = ApplicationSettings.Current.Algorithm;

			checkStartButton();
		}

		private void ApplicationSettings_FileQuantityChanged(object sender, EventArgs e) => checkStartButton();

		private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e) => saveSplitterWidth();

		private void Serialize_Finished(object sender, EventArgs e) => finished(100);

		private void Serialize_Processed(object sender, ValueProcessedEventArgs<int> args) =>  progress(args.Value);

		private void ButtonStop_Click(object sender, RoutedEventArgs e) => stop();

		private void ButtonStart_Click(object sender, RoutedEventArgs e) => start();

		#endregion

		#endregion
	}
}
