using System;

namespace SpeedrunModInstaller.Services.Exceptions
{
	public class InstallerException : Exception
	{
		public InstallerException(string message) : base(message)
		{
		}
	}
}
