using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Win32;

using Telesyk.SecuredSource.Globalization;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class FilesControl : UserControl
	{
		#region Private declarations

		private List<string> _selectedFiles = new List<string>();

		#endregion

		#region Constructors

		public FilesControl()
		{
			InitializeComponent();
		}

		#endregion

		#region Public properties

		public PackData FilePack { get; } = new PackData();

		internal bool IsMouseButtonTakeDawn { get; private set; }

		#endregion

		#region Public methods

		#endregion

		#region Overridies

		#endregion

		#region Private methods

		#region Handlers

		private void FileDialog_FileOk(object sender, CancelEventArgs e)
		{
			readDialogFiles((OpenFileDialog)sender);

			bindFiles();
		}

		private void fileControl_Select(object sender, FileControl.SelectFileEventArgs args)
		{
			selectFile(args.IsSelected, args.File);
		}

		private void ButtonFileAdd_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ButtonFileAdd.Style = (Style)FindResource("FileButtonDown");
		}

		private void ButtonFileAdd_MouseLeave(object sender, MouseEventArgs e)
		{
			ButtonFileAdd.Style = (Style)FindResource("FileButton");
		}

		private void ButtonFileAdd_MouseUp(object sender, MouseButtonEventArgs e)
		{
			ButtonFileAdd.Style = (Style)FindResource("FileButton");

			addFiles();
		}

		private void ButtonFileDelete_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ButtonFileDelete.Style = (Style)FindResource("FileButtonDeleteDown");
		}

		private void ButtonFileDelete_MouseLeave(object sender, MouseEventArgs e)
		{
			ensureDeleteButtonStyleAndState();
		}

		private void ButtonFileDelete_MouseUp(object sender, MouseEventArgs e)
		{
			deleteFiles();

			ensureDeleteButtonStyleAndState();
		}

		private void ButtonFileSelectAll_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ButtonFileSelectAll.Style = (Style)FindResource("FileButtonSelectAllDown");
		}

		private void ButtonFileSelectAll_MouseUp(object sender, MouseButtonEventArgs e)
		{
			setAllFilesSelection(true);
			ensureAllSelectableButtonsState();
		}

		private void ButtonFileSelectAll_MouseLeave(object sender, MouseEventArgs e)
		{
			ensureAllSelectableButtonsState();
		}

		private void ButtonFileDeselectAll_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ButtonFileDeselectAll.Style = (Style)FindResource("FileButtonDeselectAllDown");
		}

		private void ButtonFileDeselectAll_MouseUp(object sender, MouseButtonEventArgs e)
		{
			setAllFilesSelection(false);
			ensureAllSelectableButtonsState();
		}

		private void ButtonFileDeselectAll_MouseLeave(object sender, MouseEventArgs e)
		{
			ensureAllSelectableButtonsState();
		}

		private void PanelFiles_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				IsMouseButtonTakeDawn = true;
		}

		private void PanelFiles_MouseUp(object sender, MouseEventArgs e)
		{
			IsMouseButtonTakeDawn = false;
		}

		#endregion

		private void ensureDeleteButtonStyleAndState()
		{
			ButtonFileDelete.Style = (Style)FindResource($"FileButtonDelete{(_selectedFiles.Count == 0 ? "Disabled" : null)}");
			ButtonFileDelete.IsEnabled = _selectedFiles.Count > 0;
		}

		private void ensureAllSelectableButtonsState()
		{
			ButtonFileDeselectAll.Style = (Style)FindResource($"FileButtonDeselectAll{(_selectedFiles.Count == 0 ? "Disabled" : null)}");
			ButtonFileDeselectAll.IsEnabled = _selectedFiles.Count > 0;

			ButtonFileSelectAll.Style = (Style)FindResource($"FileButtonSelectAll{(_selectedFiles.Count == FilePack.FileCount ? "Disabled" : null)}");
			ButtonFileSelectAll.IsEnabled = _selectedFiles.Count < FilePack.FileCount;
		}

		private void setAllFilesSelection(bool isSelected)
		{
			foreach (FileControl file in PanelFiles.Children)
				file.SetSelection(isSelected);
		}

		private void selectFile(bool isSelected, FileData file)
		{
			if (isSelected)
				_selectedFiles.Add(file.FullName.ToUpper());
			else
				_selectedFiles.Remove(file.FullName.ToUpper());

			ensureDeleteButtonStyleAndState();
			ensureAllSelectableButtonsState();
		}

		private void addFiles()
		{
			var fileDialog = new OpenFileDialog();
			fileDialog.Multiselect = true;
			fileDialog.FileOk += FileDialog_FileOk;
			fileDialog.ShowDialog();

			ensureAllSelectableButtonsState();
		}

		private void deleteFiles()
		{
			foreach (FileControl file in PanelFiles.Children)
				if (file.IsSelected)
				{
					FilePack.Remove(file.File);
					_selectedFiles.Remove(file.File.FullName.ToUpper());
				}

			bindFiles();
			ensureAllSelectableButtonsState();
		}

		private void readDialogFiles(OpenFileDialog dialog)
		{
			List<FileData> files = new List<FileData>();
			List<string> withoutAccess = new List<string>();

			foreach (var fileName in dialog.FileNames)
			{
				var file = new FileData(fileName);

				if (file.MissingReadAccess)
					withoutAccess.Add(file.Name);
				else
					files.Add(file);
			}

			if (withoutAccess.Count > 0)
			{
				string list = null;

				for (var i = 0; i < withoutAccess.Count; i++)
					list += $"{i + 1}. {withoutAccess[i]}\n";

				var message = $"{Strings.MissingReadFileAccess}:\n\n{list}";

				MessageBox.Show(message, Strings.AccessMissing);
			}

			foreach (var file in files)
				FilePack.Add(file);
		}

		private void bindFiles()
		{
			PanelFiles.Children.Clear();

			foreach (var file in FilePack)
			{
				var control = new FileControl(file, this);
				PanelFiles.Children.Add(control);

				control.IsSelected = _selectedFiles.Contains(file.FullName.ToUpper());
				control.SelectionChanged += fileControl_Select;
			}

			TextFileCount.Text = FilePack.FileCount.ToString();
		}

		#endregion
	}
}
