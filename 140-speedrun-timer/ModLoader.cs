using System;
using System.Linq;
using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class ModLoader : MonoBehaviour
	{
		public static bool IsLegacyVersion { get; private set; }
		public static Settings Settings { get; private set; }

		static GameObject _modLoaderObject;
		static GameObject _modMainObject;
		static GameObject _modLevelObject;

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
			if (_modMainObject != null)
				return;

			_modMainObject = new GameObject();
			DontDestroyOnLoad(_modMainObject);

			var speedrunTimer = _modMainObject.AddComponent<SpeedrunTimer>();
			speedrunTimer.LiveSplitSyncEnabled = Settings.LiveSplitSyncEnabled;

			_modMainObject.AddComponent<UI>();
			_modMainObject.AddComponent<Updater>();
		}

		void LevelObjInit()
		{
			_modLevelObject = new GameObject();
			_modLevelObject.AddComponent<Hooks>();
			_modLevelObject.AddComponent<Misc>();
			_modLevelObject.AddComponent<Cheats>();
		}

		void OnGUI()
		{
			if (!string.IsNullOrEmpty(_errorDisplayMessage))
			{
				var style = new GUIStyle()
				{
					fontSize = UI.Scale(20),
					fontStyle = FontStyle.Bold
				};
				style.normal.textColor = Color.red;
				GUI.Label(new Rect(3, 0, Screen.width, UI.Scale(style.fontSize)),
					_errorDisplayMessage, style);
			}
		}

		public static void Inject()
		{
			if (_disabled)
				return;

			// one mod loader object instance per level
			if (_modLoaderObject != null)
				TriggerCriticalError("ModLoader.Inject() was called twice in the same level. Corrupted installation?");

			_modLoaderObject = new GameObject();
			_modLoaderObject.AddComponent<ModLoader>();
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
			Destroy(_modMainObject);
			Destroy(_modLevelObject);
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
