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
	public partial class SimpleAndFastAreaControl : UserControl
	{
		#region Private declarations

		private IProgress<int> _progress = null;
		private IProgress<int> _finish = null;
		private EncryptionPack _encryptor;
		private TickTimer _timer;

		#endregion

		#region Constructors

		public SimpleAndFastAreaControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public PackData FilePack { get => ControlFiles.FilePack; }

		public IMainWindow Host { get; set; }

		public SimpleAndFastPanelControl Panel => ControlPanel;

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
			ApplicationSettings.Current.AlgorithmChanged += ApplicationSettings_AlgorithmChanged;
			ApplicationSettings.Current.PasswordChanged += ApplicationSettings_PasswordChanged;
			ApplicationSettings.Current.FileQuantityChanged += ApplicationSettings_FileQuantityChanged;

			ColumnPanel.Width = new GridLength(ApplicationSettings.Current.PanelWidth);

			checkStartButton();

			Panel.FileChanged += (s, a) => { checkStartButton(); };
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
			ButtonStart.IsEnabled = !string.IsNullOrEmpty(Panel.FileName);
		}

		private void saveSplitterWidth()
		{
			ApplicationSettings.Current.PanelWidth = (int)ColumnPanel.ActualWidth;

			Host.MoreFeaturesArea.SetPanelWidth(ApplicationSettings.Current.PanelWidth);
		}

		private void setPanelWidth(int width)
		{
			ColumnPanel.Width = new GridLength(width);
		}

		private void start()
		{
			EncryptionPack deserializer = new EncryptionPack(Path.Combine(ApplicationSettings.Current.Directory, $"{ApplicationSettings.Current.FileName}.$$"), ControlPanel.Password, FilePack);
			deserializer.Finished += Serialize_Finished;
			deserializer.Processed += Serialize_Processed;

			ControlProgress.Value = 0;
			ControlProgress.Visibility = Visibility.Visible;
			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = false);

			_encryptor = deserializer;

			started();

			deserializer.Serialize();
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

		private void ApplicationSettings_AlgorithmChanged(object sender, EventArgs e) => checkStartButton();

		private void ApplicationSettings_FileQuantityChanged(object sender, EventArgs e) => checkStartButton();

		private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e) => saveSplitterWidth();

		private void Serialize_Finished(object sender, EventArgs e) => finished(100);

		private void Serialize_Processed(object sender, ValueProcessedEventArgs<int> args) => progress(args.Value);

		private void ButtonStop_Click(object sender, RoutedEventArgs e) => stop();

		private void ButtonStart_Click(object sender, RoutedEventArgs e) => start();

		#endregion

		#endregion
	}
}
