using System;
using System.Linq;
using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class SpeedrunTimerLoader : MonoBehaviour
	{
		public static bool IsLegacyVersion { get; private set; }

		static GameObject _modObject;

		public void Awake()
		{
			if (_modObject == null)
			{
				IsLegacyVersion = DetermineVersion();
			}

			var modLevelObject = new GameObject();
			modLevelObject.AddComponent<Hooks>();
			modLevelObject.AddComponent<Misc>();
			modLevelObject.AddComponent<Cheats>();

			if (_modObject != null)
				return;
			_modObject = new GameObject();
			_modObject.AddComponent<SpeedrunTimer>();
			DontDestroyOnLoad(_modObject);
			Destroy(this);
		}

		public static void Inject()
		{
			Globals.Instance().gameObject.AddComponent<SpeedrunTimerLoader>();
		}

		static bool DetermineVersion()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.First(a => a.FullName.StartsWith("Assembly-CSharp,"))
				.GetTypes();
			return !types.Any(t => t.Name == "GravityBoss");
		}
	}
}
