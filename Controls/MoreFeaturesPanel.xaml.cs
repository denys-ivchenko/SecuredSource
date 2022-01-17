using System;
using System.Collections.Generic;
using System.Windows.Controls;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class MoreFeaturesPanelControl : UserControl
	{
		#region Constructors

		public MoreFeaturesPanelControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public string Password => ControlPassword.Password ?? String.Empty;

		#endregion

		#region Public methods

		#endregion

		#region Private methods

		private void init()
		{
			ControlPassword.Mode = ControlMode.Encrypt;

			TextFileName.Text = ApplicationSettings.Current.FileName;
			TextFileName.TextChanged += (s, a) => ApplicationSettings.Current.FileName = TextFileName.Text;

			ControlStateOperator.Operator.RegisterForEncryptionProcess(TextFileName);
		}

		#endregion
	}
}
