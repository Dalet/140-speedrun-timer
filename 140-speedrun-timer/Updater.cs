using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class Updater : MonoBehaviour
	{
		const string updateURL = "https://raw.githubusercontent.com/Dalet/140-speedrun-timer/master/latestVersion.txt";

		public static bool NeedUpdate { get; private set; }
		public static string LatestVersion { get; private set; }

		public void Start()
		{
			StartCoroutine(CheckUpdate());
		}

		public IEnumerator CheckUpdate()
		{
			var www = new WWW(updateURL);
			yield return www;

			if (string.IsNullOrEmpty(www.error))
			{
				var str = www.text.Trim();
				var version = str != null ? new Version(str) : null;
				NeedUpdate = version != null && version > Assembly.GetExecutingAssembly().GetName().Version;
				LatestVersion = Utils.FormatVersion(version);
				Destroy(this);
			}
		}
	}
}
