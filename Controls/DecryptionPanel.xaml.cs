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
		}

		#endregion

		#region Overridies

		#endregion

		#region Public properties

		public string FileName => ControlFile.FileName;

		public Password Password => ControlPassword.Password;

		public bool IsReaded { get; private set; }

		#endregion
	}
}
