using System;
using System.Net;
using System.Reflection;

namespace SpeedrunTimerMod
{
	class Updater
	{
		const string updateURL = "https://raw.githubusercontent.com/Dalet/140-speedrun-timer/master/latestVersion.txt";

		public bool NeedUpdate { get; private set; }
		public string LatestVersion { get; private set; }

		public void GetLatestVersionAsync()
		{
			var client = new WebClient();
			try
			{
				var str = client.DownloadString(new Uri(updateURL));
				str = str?.Trim();
				var version = str != null ? new Version(str) : null;
				NeedUpdate = version != null && version > Assembly.GetExecutingAssembly().GetName().Version;
				LatestVersion = Utils.FormatVersion(version);
			}
			catch { }
			finally
			{
				client.Dispose();
			}
		}
	}
}
