using System;
using System.Windows;
using System.Windows.Navigation;

namespace Telesyk.SecuredSource
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ApplicationSettings.Current.DecryptionPackFilePath = e.Args.Length > 0 ? e.Args[0] : null;
		}
	}
}
