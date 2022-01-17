using System;

namespace Telesyk.SecuredSource.UI.Controls
{
	public interface IMainWindow
	{
		SimpleAndFastPanelControl SimpleAndFastPanel { get; }
		SimpleAndFastAreaControl SimpleAndFastArea { get; }

		MoreFeaturesPanelControl MoreFeaturesPanel { get; }
		MoreFeaturesAreaControl MoreFeaturesArea { get; }
	}
}
