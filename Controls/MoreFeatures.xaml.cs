using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Microsoft.Win32;

using Telesyk.SecuredSource.Globalization;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class MoreFeaturesAreaControl : UserControl
	{
		#region Private declarations

		private Dictionary<EncryptionAlgorythm, UserControl> _panels = new Dictionary<EncryptionAlgorythm, UserControl>();

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
			ColumnPanel.Width = new GridLength(ApplicationSettings.Current.PanelWidth);

			SelectAlgorythm.ItemsSource = Enum.GetNames(typeof(EncryptionAlgorythm));
			SelectAlgorythm.SelectedValue = ApplicationSettings.Current.Algorythm.ToString();
		}

		#region Handlers

		private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			ApplicationSettings.Current.PanelWidth = (int)ColumnPanel.ActualWidth;
		}

		private void SelectAlgorythm_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Enum.TryParse<EncryptionAlgorythm>(SelectAlgorythm.SelectedValue.ToString(), out EncryptionAlgorythm algorythm);
			ApplicationSettings.Current.Algorythm = algorythm;

			setPanelByAlgorythm();
		}

		#endregion

		private void setPanelByAlgorythm()
		{
			ensurePanelControl();

			UserControl panel = _panels[ApplicationSettings.Current.Algorythm];

			if (PanelLoad.Children.Count > 0)
				if (PanelLoad.Children[0] == panel)
					return;
				else
					PanelLoad.Children.Clear();

			PanelLoad.Children.Add(panel);
		}

		private void ensurePanelControl()
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

		#endregion
	}
}
