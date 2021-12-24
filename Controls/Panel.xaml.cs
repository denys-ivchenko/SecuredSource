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
	public partial class PanelControl : UserControl
	{
		#region Private declarations

		private Dictionary<SymmetricAlgorithmName, UserControl> _panels = new Dictionary<SymmetricAlgorithmName, UserControl>();


		#endregion

		#region Constructors

		public PanelControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public string Password { get => ControlPassword.Password; }

		#endregion

		#region Public methods

		#endregion

		#region Overridies

		#endregion

		#region Private methods

		private void init()
		{
			SelectAlgorythm.ItemsSource = Enum.GetNames(typeof(SymmetricAlgorithmName));
			SelectAlgorythm.SelectedValue = ApplicationSettings.Current.Algorythm.ToString();

			TextFileName.Text = ApplicationSettings.Current.FileName;
			TextFileName.TextChanged += (s, a) => ApplicationSettings.Current.FileName = TextFileName.Text;
		}

		private void changeAlgorythm()
		{
			Enum.TryParse(SelectAlgorythm.SelectedValue.ToString(), out SymmetricAlgorithmName algorythm);
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
					case (SymmetricAlgorithmName.Aes):
						panel = new AesPanelControl();
						break;
					case (SymmetricAlgorithmName.Rijndael):
						panel = new RijndaelPanelControl();
						break;
					case (SymmetricAlgorithmName.DES):
						panel = new DESPanelControl();
						break;
					case (SymmetricAlgorithmName.TripleDES):
						panel = new TripleDESPanelControl();
						break;
					case (SymmetricAlgorithmName.RC2):
						panel = new RC5PanelControl();
						break;
				}

				_panels.Add(ApplicationSettings.Current.Algorythm, panel);
			}
		}

		#region Handlers

		private void SelectAlgorythm_SelectionChanged(object sender, SelectionChangedEventArgs e) => changeAlgorythm();

		#endregion

		#endregion
	}
}
