using System;
using System.Windows;
using System.Windows.Controls;

using forms = System.Windows.Forms;

using Telesyk.SecuredSource.Globalization;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class SelectFileControl : UserControl
	{
		#region Constructors

		public SelectFileControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public ApplicationMode Mode { get; set; } = ApplicationMode.Encryption;

		public string FileName => TextFile.Text;

		#endregion

		#region Events

		public ValueProcessedEventHandler<string> FileChanged;

		#endregion

		#region Private methods

		private void init()
		{
			
		}

		#region Handlers

		private void ButtonSelect_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new forms.OpenFileDialog();
			dialog.Title = Strings.SelectFile;
			dialog.InitialDirectory = ApplicationSettings.Current.Directory;

			dialog.FileOk += (s, a) =>
			{
				TextFile.Text = dialog.FileName;

				if (ApplicationSettings.Current.Mode == ApplicationMode.Decryption)
					ApplicationSettings.Current.DecryptionPackPath = dialog.FileName;

				if (FileChanged != null)
					FileChanged(this, new ValueProcessedEventArgs<string>(dialog.FileName));
			};

			dialog.ShowDialog();
		}

		#endregion

		#endregion
	}
}
