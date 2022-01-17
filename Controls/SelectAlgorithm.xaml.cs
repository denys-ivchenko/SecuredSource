using System;
using System.Windows;
using System.Windows.Controls;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class SelectAlgorithmControl : UserControl
	{
		#region Private declarations

		#endregion

		#region Constructors

		public SelectAlgorithmControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		#endregion

		#region Public methods

		#endregion

		#region public events

		//public event EventHandler AlgorithmChanged;

		#endregion

		#region Private methods

		private void init()
		{
			SelectAlgorithm.ItemsSource = Enum.GetNames(typeof(CryptoAlgorithm));
			SelectAlgorithm.SelectedValue = ApplicationSettings.Current.Algorithm.ToString();

			ControlStateOperator.Operator.RegisterForEncryptionProcess(SelectAlgorithm);
		}

		private void changeAlgorythm()
		{
			Enum.TryParse(SelectAlgorithm.SelectedValue.ToString(), out CryptoAlgorithm algorythm);
			ApplicationSettings.Current.Algorithm = algorythm;
		}

		#region Handlers

		private void SelectAlgorythm_SelectionChanged(object sender, SelectionChangedEventArgs e) => changeAlgorythm();

		#endregion

		#endregion
	}
}
