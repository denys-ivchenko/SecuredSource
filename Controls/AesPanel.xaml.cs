using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class AesPanelControl : UserControl
	{
		#region Private declarations

		#endregion

		#region Constructors

		public AesPanelControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		#endregion

		#region Public methods

		#endregion

		#region Private methods

		private void init()
		{
			((RadioButton)FindName($"RadioAesPasswordLength{ApplicationSettings.Current.RequiredPasswordLength}")).IsChecked = true;
		}

		#region Handlers

		private void RadioAesPasswordLength_Checked(object sender, RoutedEventArgs e)
		{
			int.TryParse(((RadioButton)sender).Content.ToString(), out int length);
			ApplicationSettings.Current.RequiredPasswordLength = length;
		}

		#endregion

		#endregion
	}
}
