using System;

namespace SpeedrunModInstaller.Services.Exceptions
{
	public class InstallalationPathNotFoundException : InstallerException
	{
		public InstallalationPathNotFoundException()
			: base("140 is not installed at the specified location.")
		{
		}
	}
}
