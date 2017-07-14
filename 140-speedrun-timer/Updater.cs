using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	internal sealed class Updater : MonoBehaviour
	{
		const string REPO_URL = "https://raw.githubusercontent.com/Dalet/140-speedrun-timer/";
		const string UPDATE_URL = REPO_URL + "master/latestVersion.txt";
		const string UPDATE_URL_UNSTABLE = REPO_URL + "develop/latestVersion.txt";

		public static bool NeedUpdate { get; private set; }
		public static string LatestVersion { get; private set; }

		public void Start()
		{
			if (LatestVersion != null)
				return;

			StartCoroutine(CheckUpdate());
		}

		IEnumerator CheckUpdate()
		{
			yield return CheckVersion(UPDATE_URL);
#if PRE_RELEASE
			if (!NeedUpdate)
				yield return CheckVersion(UPDATE_URL_UNSTABLE);
#endif
		}

		IEnumerator CheckVersion(string url)
		{
			Version lastVersion = null;
			yield return GetVersion(UPDATE_URL, v => lastVersion = v);
			if (lastVersion == null)
				yield break;

			var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
			LatestVersion = Utils.FormatVersion(lastVersion);
			NeedUpdate = lastVersion > currentVersion;
		}

		static IEnumerator GetVersion(string url, Action<Version> callback)
		{
			var www = new WWW(url);
			yield return www;

			Version ver = null;
			if (!string.IsNullOrEmpty(www.error))
			{
				callback(ver);
				yield break;
			}

			var str = www.text.Trim();
			if (!string.IsNullOrEmpty(str))
				ver = new Version(str);

			callback(ver);
		}
	}
}
