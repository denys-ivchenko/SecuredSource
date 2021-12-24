using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Win32;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class MoreFeaturesAreaControl : UserControl
	{
		#region Private declarations

		private IProgress<int> _progress = null;

		private IProgress<int> _finish = null;

		private EncryptionPack _encryptor;


		#endregion

		#region Constructors

		public MoreFeaturesAreaControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public PackData FilePack { get => ControlFiles.FilePack; }

		#endregion

		#region Public methods

		#endregion

		#region Overridies

		#endregion

		#region Private methods

		private void init()
		{
			ControlPanel.ControlPassword.PasswordChanged += ControlPassword_PasswordChanged;
			ControlFiles.FilesChanged += ControlFiles_FilesChanged;

			ColumnPanel.Width = new GridLength(ApplicationSettings.Current.PanelWidth);

			checkStartButton();

			var lastPercentage = 0;

			_progress = new Progress<int>(percentage =>
			{
				if (percentage > lastPercentage)
				{
					if (percentage == 100)
						percentage = 100;
					
					lastPercentage = percentage;

					Debug.WriteLine($"percentage: {ControlProgress.Value = percentage}");
				}
			});

			_finish = new Progress<int>(percentage =>
			{
				lastPercentage = 0;

				ControlProgress.Value = percentage;
				ControlProgress.Visibility = Visibility.Hidden;
				ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = true);
			});
		}

		private void checkStartButton()
		{
			ButtonStart.IsEnabled = ControlPanel.ControlPassword.PasswordLength == ApplicationSettings.Current.PasswordLength && ControlFiles.FilePack.FileCount > 0;
		}

		private void saveSplitterWidth()
		{
			ApplicationSettings.Current.PanelWidth = (int)ColumnPanel.ActualWidth;
		}

		private void start()
		{
			startAsync();
		}

		private void startAsync()
		{
			Debug.WriteLine("UI.startAsync()");

			EncryptionPack serialize = new EncryptionPack(Path.Combine(ApplicationSettings.Current.Directory, $"{ApplicationSettings.Current.FileName}.$$"), ControlPanel.Password, FilePack);
			serialize.Finished += Serialize_Finished;
			serialize.Progress += Serialize_Progress;

			ControlProgress.Value = 0;
			ControlProgress.Visibility = Visibility.Visible;
			ButtonStop.IsEnabled = !(ButtonStart.IsEnabled = false);

			_encryptor = serialize;

			serialize.Process();

			Debug.WriteLine("UI.startAsync.");
		}

		private void stop()
		{
			//try { _encryptor.Cancel(); }
			//catch { }
			Debug.WriteLine("UI.stop()");
			_encryptor.Cancel();
			Debug.WriteLine("UI.stop.");
		}

		#region Handlers

		private void ControlPassword_PasswordChanged(object sender, EventArgs e) => checkStartButton();

		private void ControlFiles_FilesChanged(object sender, EventArgs e) => checkStartButton();

		private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e) => saveSplitterWidth();

		private void Serialize_Finished(object sender, EventArgs e) => _finish.Report(0);

		private void Serialize_Progress(object sender, ProgressEventArgs e) => _progress.Report(e.Percentage);

		private void ButtonStop_Click(object sender, RoutedEventArgs e) => stop();

		private void ButtonStart_Click(object sender, RoutedEventArgs e) => start();

		#endregion

		#endregion
	}
}
