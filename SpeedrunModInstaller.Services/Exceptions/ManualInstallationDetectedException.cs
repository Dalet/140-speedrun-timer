using System;

namespace SpeedrunModInstaller.Services.Exceptions
{
	public class UnableToUninstallException : InstallerException
	{
		public UnableToUninstallException()
			: base("Unable to uninstall the mod.")
		{
		}
	}
}
