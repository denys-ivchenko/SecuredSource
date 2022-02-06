using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class EncryptionPanelControl : UserControl
	{
		#region Constructors

		public EncryptionPanelControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Overridies

		#endregion

		#region Private methods

		private void init()
		{
			ControlEncryptionPassword.Mode = ApplicationMode.Encryption;

			TextFileName.Text = ApplicationSettings.Current.FileName;
			TextFileName.TextChanged += (s, a) => ApplicationSettings.Current.FileName = TextFileName.Text;

			ApplicationOperator.Operator.RegisterForEncryptionProcess(TextFileName);
		}

		#endregion
	}
}
