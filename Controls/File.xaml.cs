using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class FileControl : UserControl
	{
		#region Private declarations

		private bool _isSelected;
		private bool _skipUnselectOnce;

		#endregion

		#region Constructors

		public FileControl(FileData file, FilesControl filesArea)
		{
			FilesArea = filesArea;
			
			InitializeComponent();

			init(file);
		}

		#endregion

		#region Public properties

		public FileData File { get; private set; }

		public bool IsSelected 
		{ 
			get => _isSelected;
			internal set
			{
				_isSelected = value;
				setStyle();
			}
		}

		internal FilesControl FilesArea { get; private set; }

		#endregion

		#region Public methods

		public void SetSelection(bool selected)
		{
			if (selected != IsSelected)
			{
				IsSelected = selected;

				if (SelectionChanged != null)
					SelectionChanged(this, new SelectFileEventArgs(selected, File));
			}
		}

		#endregion

		#region Events

		public event SelectFileEventHandler SelectionChanged;

		#endregion

		#region Private methods

		#region Handlers

		private void Container_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (_skipUnselectOnce)
			{
				_skipUnselectOnce = false;
				return;
			}
			
			IsSelected = !IsSelected;

			setStyle();

			if (SelectionChanged != null)
				SelectionChanged(this, new SelectFileEventArgs(IsSelected, File));
		}

		private void Container_MouseMove(object sender, MouseEventArgs e)
		{
			setStyle();

			if (e.LeftButton == MouseButtonState.Pressed /*&& FilesArea.IsMouseButtonTakeDawn*/ && !IsSelected)
			{
				IsSelected = _skipUnselectOnce = true;

				if (SelectionChanged != null)
					SelectionChanged(this, new SelectFileEventArgs(IsSelected, File));
			}
		}

		private void Container_MouseLeave(object sender, MouseEventArgs e)
		{
			setStyle();

			_skipUnselectOnce = false;
		}

		#endregion

		private void init(FileData file)
		{
			File = file;
			Text.Text = File.Name;
			Text.ToolTip = File.FullName;
		}

		private void setStyle() => setStyle(null);

		private void setStyle(string suffix)
		{
			Container.Style = (Style)FindResource($"File{(IsSelected ? "Selected" : null)}");
		}

		#endregion

		public delegate void SelectFileEventHandler(object sender, SelectFileEventArgs args);

		public class SelectFileEventArgs : EventArgs
		{
			public SelectFileEventArgs(bool selected, FileData file)
			{
				IsSelected = selected;
				File = file;
			}

			public bool IsSelected { get; private set; }

			public FileData File { get; private set; }
		}

		public enum Position
		{
			First,
			Odd,
			Even
		}
	}
}
