using System;
using System.Windows.Controls;
using System.Windows.Input;

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

		#region Private methods

		private void init()
		{
			TextDirectory.Text = ApplicationSettings.Current.Directory;
		}

		#region Handlers

		private void ButtonBrowse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
