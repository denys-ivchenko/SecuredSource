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
	public abstract class AreaControl : UserControl
	{
		static AreaControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AreaControl), new FrameworkPropertyMetadata(typeof(AreaControl)));
		}

		public abstract ApplicationMode TargetMode { get; }
	}
}
