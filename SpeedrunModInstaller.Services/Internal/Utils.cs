using System;
using System.Diagnostics;
using System.IO;
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

		internal static void Foo(string path, [CallerMemberName] string caller = "")
		{
			//var path = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\140\\140_Data\\Managed\\System.dll";
			//path = path ?? "C:\\Program Files (x86)\\Steam\\steamapps\\common\\140\\140_Data\\Managed\\Assembly-CSharp.dll";
			try
			{
				using(File.OpenRead(path)){}

				try
				{
					using (File.OpenWrite(path)){}
				}
				catch
				{
					Trace.TraceWarning($"{caller} could not open WRITE {Path.GetFileName(path)}");
				}
			}
			catch
			{
				Trace.TraceWarning($"{caller} could not open READ {Path.GetFileName(path)}");
			}
		}
	}
}
