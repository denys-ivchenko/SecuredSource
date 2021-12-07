using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Telesyk.SecuredSource.UI.Controls
{
	public class MoreFeaturesMainAreaControl : AreaControl
	{
		static MoreFeaturesMainAreaControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MoreFeaturesMainAreaControl), new FrameworkPropertyMetadata(typeof(MoreFeaturesMainAreaControl)));
		}

		public override ApplicationMode TargetMode => ApplicationMode.MoreFeatures;
	}
}
