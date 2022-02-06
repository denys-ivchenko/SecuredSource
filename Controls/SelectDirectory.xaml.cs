using System;
using System.Windows;
using System.Windows.Controls;

using forms = System.Windows.Forms;

using Telesyk.SecuredSource.Globalization;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class SelectDirectoryControl : UserControl
	{
		#region Constructors

		public SelectDirectoryControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public ApplicationMode Mode { get; set; } = ApplicationMode.Encryption;

		#endregion

		#region Private methods

		private void init()
		{
			TextDirectory.Text = Mode == ApplicationMode.Encryption ? ApplicationSettings.Current.EncryptionDirectory : ApplicationSettings.Current.DecryptionDirectory;

			ApplicationOperator.Operator.RegisterForEncryptionProcess(TextDirectory, ButtonSelect);

			ApplicationSettings.Current.ModeChanged += (s, a) => setMode();
		}

		private void setMode()
		{
			TextDirectory.Text = Mode == ApplicationMode.Encryption ? ApplicationSettings.Current.EncryptionDirectory : ApplicationSettings.Current.DecryptionDirectory;
		}

		public override void OnApplyTemplate() => TextTitle.Text = Mode == ApplicationMode.Encryption ? Strings.SaveDirectory : Strings.UploadDirectory;

		#region Handlers

		private void ButtonSelect_Click(object sender, RoutedEventArgs e)
		{
			forms.FolderBrowserDialog dialog = new forms.FolderBrowserDialog();
			dialog.Description = Strings.SelectSavingDirectory;
			dialog.SelectedPath = Mode == ApplicationMode.Encryption ? ApplicationSettings.Current.EncryptionDirectory : ApplicationSettings.Current.DecryptionDirectory;
			dialog.ShowDialog();

			TextDirectory.Text = dialog.SelectedPath;

			if (Mode == ApplicationMode.Encryption)
				ApplicationSettings.Current.EncryptionDirectory = dialog.SelectedPath;
			else
				ApplicationSettings.Current.DecryptionDirectory = dialog.SelectedPath;
		}

		#endregion

		#endregion
	}
}
