using System;
using System.Windows;
using System.Windows.Controls;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class SelectAlgorithmControl : UserControl
	{
		#region Constructors

		public SelectAlgorithmControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		private ApplicationMode _mode;

		public ApplicationMode Mode 
		{ 
			get => _mode;
			set => _mode = value; 
		}

		//public event EventHandler AlgorithmChanged;

		#endregion

		#region Public events

		//public event EventHandler AlgorithmChanged;

		#endregion

		#region Private methods

		private void init()
		{
			SelectAlgorithm.ItemsSource = Enum.GetNames(typeof(CryptoAlgorithm));

			ControlStateOperator.Operator.RegisterForEncryptionProcess(SelectAlgorithm);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			SelectAlgorithm.SelectedValue = $"{(Mode == ApplicationMode.Encryption ? ApplicationSettings.Current.Algorithm : ApplicationSettings.Current.DecryptionAlgorithm)}";
		}

		private void changeAlgorythm()
		{
			Enum.TryParse(SelectAlgorithm.SelectedValue.ToString(), out CryptoAlgorithm algorythm);

			if (Mode == ApplicationMode.Encryption)
				ApplicationSettings.Current.Algorithm = algorythm;
			else
				ApplicationSettings.Current.DecryptionAlgorithm = algorythm;
		}

		#region Handlers

		private void SelectAlgorythm_SelectionChanged(object sender, SelectionChangedEventArgs e) => changeAlgorythm();

		#endregion

		#endregion
	}
}
