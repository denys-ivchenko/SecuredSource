using System;

namespace Telesyk.SecuredSource.UI.Controls
{
	public interface IMainWindow
	{
		DecryptionPanelControl SimpleAndFastPanel { get; }
		DecryptionAreaControl DecryptionArea { get; }

		EncryptionPanelControl MoreFeaturesPanel { get; }
		EncryptionAreaControl EncryptionArea { get; }
	}
}
