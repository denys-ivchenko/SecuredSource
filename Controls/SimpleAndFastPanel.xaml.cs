using System;
using System.Collections.Generic;
using System.Windows.Controls;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class SimpleAndFastPanelControl : UserControl
	{
		#region Constructors

		public SimpleAndFastPanelControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public string FileName => ControlFile.FileName;

		public string Password => ControlPassword.Password ?? String.Empty;

		public bool IsReaded { get; private set; }

		#endregion

		#region Events

		public ValueProcessedEventHandler<string> FileChanged;

		#endregion

		#region Private methods

		private void init()
		{
			ControlPassword.Mode = ControlMode.Decrypt;

			ControlFile.FileChanged += (s, a) => checkMustRead();
		}

		private void checkMustRead()
		{

		}

		#endregion

		private void ButtonLoad_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			TextProcess.Text = "Deserializing...";
			
			var pack = new EncryptionPack(ControlFile.FileName, ControlPassword.Password, ApplicationSettings.Current.Directory);

			pack.Finished += (s, a) =>
			{
				TextProcess.Text = null;
			};

			pack.Deserialize();
		}

		private void Pack_Finished(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
