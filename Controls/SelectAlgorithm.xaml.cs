using System;
using System.Windows;
using System.Windows.Controls;

using Telesyk.Cryptography;

using oper = Telesyk.SecuredSource.ApplicationOperator;

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

		#region Public methods

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

		#region Private methods

		private void init()
		{
			SelectAlgorithm.ItemsSource = Enum.GetNames(typeof(SymmetricAlgorithmName));

			oper.Operator.RegisterForEncryptionProcess(SelectAlgorithm);

			if (Mode == ApplicationMode.Decryption)
				oper.Operator.DeserializePackChanged += (s, a) =>
				{
					if (oper.Operator.DeserializePack != null)
						SelectAlgorithm.SelectedValue = $"{oper.Operator.DeserializePack.FilePack.Algorithm}";
				};
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			SelectAlgorithm.SelectedValue = $"{(Mode == ApplicationMode.Encryption ? ApplicationSettings.Current.EncryptionAlgorithm : ApplicationSettings.Current.DecryptionAlgorithm)}";
		}

		private void changeAlgorythm()
		{
			Enum.TryParse(SelectAlgorithm.SelectedValue.ToString(), out SymmetricAlgorithmName algorithm);

			if (Mode == ApplicationMode.Encryption)
				ApplicationSettings.Current.SetEncryptionAlgorithm(algorithm);
			else
				ApplicationSettings.Current.SetDecryptionAlgorithm(algorithm);
		}

		#region Handlers

		private void SelectAlgorythm_SelectionChanged(object sender, SelectionChangedEventArgs e) => changeAlgorythm();

		#endregion

		#endregion
	}
}
