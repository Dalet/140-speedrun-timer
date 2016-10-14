using System;

namespace SpeedrunTimerModInstaller
{
	static class Utils
	{
		public static bool IsUnix()
		{
			var p = (int)Environment.OSVersion.Platform;
			return p == 4 || p == 6 || p == 128;
		}
	}
}
