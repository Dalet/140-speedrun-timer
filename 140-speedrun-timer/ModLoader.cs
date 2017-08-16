using System;
using System.Linq;
using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class ModLoader : MonoBehaviour
	{
		public static bool IsLegacyVersion { get; private set; }
		public static Settings Settings { get; private set; }

		public static GameObject ModLoaderObject { get; private set; }
		public static GameObject ModMainObject { get; private set; }
		public static GameObject ModLevelObject { get; private set; }

		static bool _disabled;
		static string _errorDisplayMessage;

		void Awake()
		{
			ModInit();

			if (_disabled)
				return;

			ModObjInit();
			LevelObjInit();
		}

		void ModInit()
		{
			if (Settings != null)
				return;

			IsLegacyVersion = CheckIfLegacy();
			Settings = new Settings();
			if (!Settings.Load())
				Debug.Log("Failed to load Speedrun Timer Mod settings.");

			if (Settings.ModDisabled)
			{
				_disabled = true;
			}
		}

		void ModObjInit()
		{
			if (ModMainObject != null)
				return;

			Resources.Instance.LoadAllResources();

			ModMainObject = new GameObject();
			DontDestroyOnLoad(ModMainObject);

			var speedrunTimer = ModMainObject.AddComponent<SpeedrunTimer>();
			speedrunTimer.LiveSplitSyncEnabled = Settings.LiveSplitSyncEnabled;

			ModMainObject.AddComponent<UI>();
			ModMainObject.AddComponent<Updater>();
		}

		void LevelObjInit()
		{
			ModLevelObject = new GameObject();
			ModLevelObject.AddComponent<Hooks>();
			ModLevelObject.AddComponent<Misc>();
			ModLevelObject.AddComponent<Cheats>();
		}

		void OnGUI()
		{
			if (!string.IsNullOrEmpty(_errorDisplayMessage))
			{
				var style = new GUIStyle()
				{
					fontSize = UI.ScaleVertical(20),
					fontStyle = FontStyle.Bold
				};
				style.normal.textColor = Color.red;
				GUI.Label(new Rect(3, 0, Screen.width, UI.ScaleVertical(style.fontSize)),
					_errorDisplayMessage, style);
			}
		}

		public static void Inject()
		{
			if (_disabled)
				return;

			// one mod loader object instance per level
			if (ModLoaderObject != null)
				TriggerCriticalError("ModLoader.Inject() was called twice in the same level. Corrupted installation?");

			ModLoaderObject = new GameObject();
			ModLoaderObject.AddComponent<ModLoader>();
		}

		public static void TriggerCriticalError(string exceptionMsg, string displayMsg = null)
		{
			if (displayMsg == null)
				displayMsg = "Speedrun Timer Mod encountered a critical error, check output_log.txt";

			_errorDisplayMessage = displayMsg;
			_disabled = true;
			DestroyModObjects();
			throw new Exception("Speedrun Timer Mod Critical Error\n" + exceptionMsg);
		}

		public static void DestroyModObjects()
		{
			Destroy(ModMainObject);
			Destroy(ModLevelObject);
		}

		static bool CheckIfLegacy()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.First(a => a.FullName.StartsWith("Assembly-CSharp,"))
				.GetTypes();
			return !types.Any(t => t.Name == "GravityBoss");
		}
	}
}
