using System;
using System.Windows;
using System.Windows.Controls;

using forms = System.Windows.Forms;

using Telesyk.SecuredSource.Globalization;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class SelectDirectoryControl : UserControl
	{
		#region Private fields

		private ControlMode _mode = ControlMode.Encrypt;

		#endregion

		#region Constructors

		public SelectDirectoryControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public ControlMode Mode
		{
			get => _mode;
			set
			{
				_mode = value;

				ensureMode();
			}
		}

		#endregion

		#region Private methods

		private void init()
		{
			TextDirectory.Text = ApplicationSettings.Current.Directory;

			ControlStateOperator.Operator.RegisterForEncryptionProcess(TextDirectory, ButtonSelect);
		}

		private void ensureMode()
		{
			TextTitle.Text = Mode == ControlMode.Encrypt ? Strings.SaveDirectory : Strings.UploadDirectory;
		}

		#region Handlers

		private void ButtonSelect_Click(object sender, RoutedEventArgs e)
		{
			forms.FolderBrowserDialog dialog = new forms.FolderBrowserDialog();
			dialog.Description = Strings.SelectSavingDirectory;
			dialog.SelectedPath = ApplicationSettings.Current.Directory;
			dialog.ShowDialog();

			TextDirectory.Text = ApplicationSettings.Current.Directory = dialog.SelectedPath;
		}

		#endregion

		#endregion
	}
}
