using System;
using System.Linq;
using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class ModLoader : MonoBehaviour
	{
		public static bool IsLegacyVersion { get; private set; }
		public static Settings Settings { get; private set; }

		public static GameObject LoaderObject { get; private set; }
		public static GameObject MainObject { get; private set; }
		public static GameObject LevelObject { get; private set; }

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
				return;
			}

			Application.targetFrameRate = Settings.TargetFramerate;
			Application.runInBackground = Settings.RunInBackground;
			QualitySettings.vSyncCount = Settings.Vsync ? 1 : 0;
		}

		void ModObjInit()
		{
			if (MainObject != null)
				return;

			Resources.Instance.LoadAllResources();

			MainObject = new GameObject();
			DontDestroyOnLoad(MainObject);

			MainObject.AddComponent<DebugBeatListener>();
			var speedrunTimer = MainObject.AddComponent<SpeedrunTimer>();
			speedrunTimer.LiveSplitSyncEnabled = Settings.LiveSplitSyncEnabled;
			MainObject.AddComponent<UI>();
			MainObject.AddComponent<Updater>();
		}

		void LevelObjInit()
		{
			LevelObject = new GameObject();
            LevelObject.AddComponent<GameObserversManager>();
			LevelObject.AddComponent<ResetHotkey>();
			LevelObject.AddComponent<Cheats>();
			LevelObject.AddComponent<PlayerControlOverride>();
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
			if (LoaderObject != null)
				TriggerCriticalError("ModLoader.Inject() was called twice in the same level. Corrupted installation?");

			LoaderObject = new GameObject();
			LoaderObject.AddComponent<ModLoader>();
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
			Destroy(MainObject);
			Destroy(LevelObject);
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
