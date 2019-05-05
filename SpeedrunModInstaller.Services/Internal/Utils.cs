using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SpeedrunModInstaller.Services.Internal
{
	internal static class Utils
	{
		internal static bool IsUnix()
		{
			var p = (int) Environment.OSVersion.Platform;
			return p == 4 || p == 6 || p == 128;
		}

		internal static void TraceInfo(string info, [CallerMemberName] string caller = "")
		{
			Trace.TraceInformation($"{caller} | {info}");
		}
	}
}
