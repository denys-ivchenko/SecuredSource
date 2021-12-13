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
	/// <summary>
	/// Логика взаимодействия для ProgressControl.xaml
	/// </summary>
	public partial class ProgressControl : UserControl
	{
		#region Private declarations

		private int _value;

		#endregion

		#region Constructors

		public ProgressControl()
		{
			InitializeComponent();
		}
		
		#endregion

		#region Public properties

		public int Value 
		{ 
			get => _value;
			set => TextPercentage.Text = $"{Progress.Value = _value = value}"; 
		}

		#endregion

		#region Public methods

		#endregion

		#region Overridies

		#endregion

		#region Overrided methods

		#endregion

		#region Private methods

		#endregion
	}
}
