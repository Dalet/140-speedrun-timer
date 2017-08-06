using System;
using System.Linq;
using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class ModLoader : MonoBehaviour
	{
		public static bool IsLegacyVersion { get; private set; }

		public static Settings Settings { get; private set; }

		static GameObject _modObject;

		void Awake()
		{
			ModInit();
			LevelInit();
		}

		void ModInit()
		{
			if (_modObject != null)
				return;

			_modObject = new GameObject();
			DontDestroyOnLoad(_modObject);

			IsLegacyVersion = DetermineVersion();
			Settings = new Settings();
			if (!Settings.Load())
				Debug.Log("Failed to load Speedrun Timer Mod settings.");

			var speedrunTimer = _modObject.AddComponent<SpeedrunTimer>();
			speedrunTimer.LiveSplitSyncEnabled = Settings.LiveSplitSyncEnabled;

			_modObject.AddComponent<UI>();
			_modObject.AddComponent<Updater>();
		}

		void LevelInit()
		{
			var modLevelObject = new GameObject();
			modLevelObject.AddComponent<Hooks>();
			modLevelObject.AddComponent<Misc>();
			modLevelObject.AddComponent<Cheats>();
		}

		public static void Inject()
		{
			Globals.Instance().gameObject.AddComponent<ModLoader>();
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
