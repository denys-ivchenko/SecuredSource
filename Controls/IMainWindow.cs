using System;

namespace Telesyk.SecuredSource.UI.Controls
{
	public interface IMainWindow
	{
		DecryptionPanelControl DecryptionPanel { get; }
		DecryptionAreaControl DecryptionArea { get; }

		EncryptionPanelControl EncryptionPanel { get; }
		EncryptionAreaControl EncryptionArea { get; }
	}
}
