using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Threading.Tasks;

using Microsoft.Win32;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class MoreFeaturesAreaControl : UserControl
	{
		#region Private declarations

		private Dictionary<EncryptionAlgorythm, UserControl> _panels = new Dictionary<EncryptionAlgorythm, UserControl>();

		private int _totalPercentage = 0;

		private IProgress<int> _progress = null;

		private IProgress<int> _complette = null;


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
			ControlPassword.PasswordChanged += ControlPassword_PasswordChanged;
			
			ColumnPanel.Width = new GridLength(ApplicationSettings.Current.PanelWidth);
			TextDirectory.Text = ApplicationSettings.Current.Directory;

			SelectAlgorythm.ItemsSource = Enum.GetNames(typeof(EncryptionAlgorythm));
			SelectAlgorythm.SelectedValue = ApplicationSettings.Current.Algorythm.ToString();

			checkEncryptButton();

			_progress = new Progress<int>(percentage =>
			{
				ControlProgress.Value = percentage;
			});

			_complette = new Progress<int>(percentage =>
			{
				ControlProgress.Visibility = Visibility.Hidden;
				ButtonEncrypt.IsEnabled = true;
			});
		}

		private void changeAlgorythm()
		{
			Enum.TryParse<EncryptionAlgorythm>(SelectAlgorythm.SelectedValue.ToString(), out EncryptionAlgorythm algorythm);
			ApplicationSettings.Current.Algorythm = algorythm;

			setPanelByAlgorythm();
		}

		private void setPanelByAlgorythm()
		{
			checkPanelInList();

			UserControl panel = _panels[ApplicationSettings.Current.Algorythm];

			if (PanelLoad.Children.Count > 0)
				if (PanelLoad.Children[0] == panel)
					return;
				else
					PanelLoad.Children.Clear();

			PanelLoad.Children.Add(panel);
		}

		private void checkPanelInList()
		{
			if (!_panels.ContainsKey(ApplicationSettings.Current.Algorythm))
			{
				UserControl panel = null;

				switch (ApplicationSettings.Current.Algorythm)
				{
					case (EncryptionAlgorythm.Aes):
						panel = new AesPanelControl();
						break;
					case (EncryptionAlgorythm.Rijndael):
						panel = new RijndaelPanelControl();
						break;
					case (EncryptionAlgorythm.DES):
						panel = new DESPanelControl();
						break;
					case (EncryptionAlgorythm.TripleDES):
						panel = new TripleDESPanelControl();
						break;
					case (EncryptionAlgorythm.RC5):
						panel = new RC5PanelControl();
						break;
				}

				_panels.Add(ApplicationSettings.Current.Algorythm, panel);
			}
		}

		private void checkEncryptButton()
		{
			ButtonEncrypt.IsEnabled = ControlPassword.PasswordLength == ApplicationSettings.Current.PasswordLength;
		}

		private void saveSplitterWidth()
		{
			ApplicationSettings.Current.PanelWidth = (int)ColumnPanel.ActualWidth;
		}

		private async Task start()
		{
			EncryptionPack serialize = new EncryptionPack(@"D:\Проекты\Secured Source\Testing\data.$$", " Slava Ukraine! ", FilePack);
			serialize.Completted += Serialize_Completted;
			serialize.Progress += Serialize_Progress;

			ControlProgress.Visibility = Visibility.Visible;
			ButtonEncrypt.IsEnabled = false;

			await Task.Run(() => { serialize.Process(); });

			ButtonEncrypt.IsEnabled = true;
		}

		#region Handlers

		private void SelectAlgorythm_SelectionChanged(object sender, SelectionChangedEventArgs e) => changeAlgorythm();

		private void ControlPassword_PasswordChanged(object sender, EventArgs e) => checkEncryptButton();

		private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e) => saveSplitterWidth();

		private void Button_MouseUp(object sender, MouseButtonEventArgs e)
		{
			var t = start();
			var c = t.IsCanceled;
		}

		private void Serialize_Completted(object sender, EventArgs e)
		{
			_complette.Report(100);
		}

		private void Serialize_Progress(object sender, ProgressEventArgs e)
		{
			_progress.Report(e.Percentage);
		}

		#endregion

		#endregion

		private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			
		}
	}
}
