using System.IO;
using System.Windows;
using System.Windows.Controls;

using forms = System.Windows.Forms;

using Telesyk.SecuredSource.Globalization;

using oper = Telesyk.SecuredSource.ApplicationOperator;

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

		#region Private methods

		private void init()
		{
			oper.Operator.RegisterForEncryptionProcess(this);

			oper.Operator.DeserializePackChanged += (s, a) =>
			{
				if (TextFile.Text?.ToUpper() != (oper.Operator.DeserializePack?.FilePath ?? string.Empty).ToUpper())
					TextFile.Text = oper.Operator.DeserializePack?.FilePath;
			};
		}

		#region Handlers

		private void ButtonSelect_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new forms.OpenFileDialog();
			dialog.Title = Strings.SelectFile;
			dialog.InitialDirectory = ApplicationSettings.Current.EncryptionDirectory;

			dialog.FileOk += (s, a) =>
			{
				if (!a.Cancel && ApplicationSettings.Current.Mode == ApplicationMode.Decryption)
				{
					TextFile.Text = dialog.FileName;

					try { oper.Operator.LoadDeserializePack(dialog.FileName); }
					catch { }

					if (oper.Operator.DeserializePack == null || !oper.Operator.DeserializePack.IsValid)
					{
						MessageBox.Show(Application.Current.MainWindow, Strings.IncorrectPackFile, Strings.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);

						TextFile.Text = null;
					}
				}
			};

			dialog.ShowDialog();
		}

		#endregion

		#endregion
	}
}
