using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using oper = Telesyk.SecuredSource.ApplicationOperator;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class EncryptionAreaControl : UserControl
	{
		#region Private declarations

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

		public IMainWindow Host { get; set; }

		public EncryptionPanelControl Panel => ControlPanel;

		#endregion

		#region Protected properties

		protected int LastPercentage;

		#endregion

		#region Public methods

		public void SetPanelWidth(int width) => setPanelWidth(width);

		#endregion

		#region Private methods

		private void init()
		{
			ApplicationSettings.Current.EncryptionAlgorithmChanged += ApplicationSettings_AlgorithmChanged;
			ApplicationSettings.Current.EncryptionPasswordChanged += ApplicationSettings_PasswordChanged;
			ApplicationSettings.Current.FileQuantityChanged += ApplicationSettings_FileQuantityChanged;

			//ControlFiles.FilesChanged += ControlFiles_FilesChanged;

			ColumnPanel.Width = new GridLength(ApplicationSettings.Current.PanelWidth);

			checkStartButton();
		}

		private void progress(int percentage)
		{
			if (percentage > LastPercentage)
			{
				LastPercentage = percentage;

				Debug.WriteLine($"percentage: {ControlProgress.Value = percentage}");
			}
		}

		private void checkStartButton()
		{
			ButtonStart.IsEnabled = ApplicationSettings.Current.EncryptionPassword?.SymbolCount > 0 && oper.Operator.EncryptionPack.FileCount > 0;
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
			oper.Operator.SerializePack = new EncryptionPack(Path.Combine(ApplicationSettings.Current.EncryptionDirectory, $"{ApplicationSettings.Current.FileName}.$$"), oper.Operator.EncryptionPack);

			oper.Operator.SerializePack.Finished += Serialize_Finished;
			oper.Operator.SerializePack.Processed += Serialize_Processed;

			ControlProgress.Value = 0;
			ControlProgress.Visibility = Visibility.Visible;
			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = false);

			_timer = new TickTimer(1000, writeTime);
			_timer.Start();

			oper.Operator.SerializePack.Serialize();
		}

		private void writeTime(TickTimer timer)
		{
			TextTime.Text = timer.LastTickEllapsed.ToString(@"hh\:mm\:ss");
		}

		private void finished(int percentage)
		{
			_timer.Stop(true);
			
			LastPercentage = 0;

			TextTime.Text = null;

			ControlProgress.Value = percentage;
			ControlProgress.Visibility = Visibility.Hidden;
			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = true);

			if (oper.Operator.SerializePack.IsFaulted)
				MessageBox.Show(oper.Operator.SerializePack.Error.Message, oper.Operator.SerializePack.Error.GetType().Name);
		}

		private void stop() => oper.Operator.SerializePack.Cancel();

		#region Handlers

		private void ApplicationSettings_PasswordChanged(object sender, EventArgs e) => checkStartButton();

		private void ApplicationSettings_AlgorithmChanged(object sender, EventArgs e) => checkStartButton();

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
