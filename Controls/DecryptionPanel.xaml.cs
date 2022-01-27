using System;
using System.Collections.Generic;
using System.Windows.Controls;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class DecryptionPanelControl : UserControl
	{
		#region Constructors

		public DecryptionPanelControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public string FileName => ControlFile.FileName;

		public string Password => ControlPassword.Password ?? String.Empty;

		public string PasswordHash => ControlPassword.PasswordHash;

		public bool IsReaded { get; private set; }

		#endregion

		#region Events

		public event ValueProcessedEventHandler<string> PasswordChanged;

		public event ValueProcessedEventHandler<string> PasswordHashChanged;

		#endregion

		#region Private methods

		private void init()
		{
			ControlPassword.PasswordChanged += (s, a) => PasswordChanged?.Invoke(s, a);
			ControlPassword.PasswordHashChanged += (s, a) => PasswordHashChanged?.Invoke(s, a);
		}

		#endregion
	}
}
