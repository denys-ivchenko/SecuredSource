using System;
using System.Collections.Generic;
using System.Windows.Controls;

using Telesyk.Cryptography;

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

		#region Public properties

		public string Password => ControlPassword.Password ?? String.Empty;

		public byte[] PasswordBytes => ControlPassword.PasswordBytes;

		public byte[] PasswordHashBytes => ControlPassword.PasswordHashBytes;

		public string PasswordHash => ControlPassword.PasswordHash;

		#endregion

		#region Public methods

		#endregion

		#region Private methods

		private void init()
		{
			ControlPassword.Mode = ApplicationMode.Encryption;

			TextFileName.Text = ApplicationSettings.Current.FileName;
			TextFileName.TextChanged += (s, a) => ApplicationSettings.Current.FileName = TextFileName.Text;

			ControlStateOperator.Operator.RegisterForEncryptionProcess(TextFileName);
		}

		#endregion
	}
}
