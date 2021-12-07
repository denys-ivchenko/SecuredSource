using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
//using System.Windows.Forms;
using System.Windows.Documents;

using Microsoft.Win32;

using Telesyk.SecuredSource;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class MoreFeatures
	{
		#region Private declarations

		#endregion

		#region Constructors

		#endregion

		#region Public properties

		#endregion

		#region Public methods

		#endregion

		#region Overridies

		#endregion

		#region Private methods

		private void init(Grid grid)
		{
			var ColumnPanel = (ColumnDefinition)grid.FindName("ColumnPanel");
			ColumnPanel.Width = new GridLength(ApplicationSettings.Current.LastPanelWidth);

			var selectAlgorythm = (ComboBox)grid.FindName("SelectAlgorythm");
			selectAlgorythm.ItemsSource = Enum.GetNames(typeof(EncryptionAlgorythm));
			selectAlgorythm.SelectedValue = ApplicationSettings.Current.LastAlgorythm.ToString();

			((RadioButton)grid.FindName($"RadioAesPasswordLength{ApplicationSettings.Current.LastAesPasswordLength}")).IsChecked = true;
		}

		#region Handlers

		private void Grid_Initialized(object sender, EventArgs e)
		{
			init((Grid)sender);
		}

		private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			var splitter = ((GridSplitter)sender);
			var parent = (Grid)splitter.Parent;

			ApplicationSettings.Current.LastPanelWidth = (int)((ColumnDefinition)parent.FindName("ColumnPanel")).ActualWidth;
		}

		private void SelectAlgorythm_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Enum.TryParse<EncryptionAlgorythm>(((ComboBox)sender).SelectedValue.ToString(), out EncryptionAlgorythm algorythm);
			ApplicationSettings.Current.LastAlgorythm = algorythm;
		}

		private void RadioAesPasswordLength_Checked(object sender, RoutedEventArgs e)
		{
			int.TryParse(((RadioButton)sender).Content.ToString(), out int length);
			ApplicationSettings.Current.LastAesPasswordLength = length;
		}

		#endregion

		#endregion
	}
}
