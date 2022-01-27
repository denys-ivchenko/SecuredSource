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
			TextDirectory.Text = Mode == ApplicationMode.Encryption ? ApplicationSettings.Current.Directory : ApplicationSettings.Current.DecryptionDirectory;

			ControlStateOperator.Operator.RegisterForEncryptionProcess(TextDirectory, ButtonSelect);
		}

		public override void OnApplyTemplate() => TextTitle.Text = Mode == ApplicationMode.Encryption ? Strings.SaveDirectory : Strings.UploadDirectory;

		#region Handlers

		private void ButtonSelect_Click(object sender, RoutedEventArgs e)
		{
			forms.FolderBrowserDialog dialog = new forms.FolderBrowserDialog();
			dialog.Description = Strings.SelectSavingDirectory;
			dialog.SelectedPath = Mode == ApplicationMode.Encryption ? ApplicationSettings.Current.Directory : ApplicationSettings.Current.DecryptionDirectory;
			dialog.ShowDialog();

			TextDirectory.Text = dialog.SelectedPath;

			if (ApplicationSettings.Current.Mode == ApplicationMode.Encryption)
				ApplicationSettings.Current.Directory = dialog.SelectedPath;
			else
				ApplicationSettings.Current.DecryptionDirectory = dialog.SelectedPath;
		}

		#endregion

		#endregion
	}
}
