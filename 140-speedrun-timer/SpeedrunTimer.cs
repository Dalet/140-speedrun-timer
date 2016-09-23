using UnityEngine;

namespace SpeedrunTimerMod
{
	public sealed class SpeedrunTimer : MonoBehaviour
    {
		static SpeedrunTimer instance;
		public static SpeedrunTimer Instance => instance;

		static bool isGameTimePaused = true;

		static double gameTime;
		static float realTime;
		static float speedrunStartRealTime = 0f;
		static float speedrunEndRealTime = 0f;

		static int nextActionFrame = -1;
		static ActionDel nextAction;
		public delegate void ActionDel();

		static Updater updater;

		static bool hideTimer = false;
		static GUIStyle updateStyle;
		static GUIStyle timerStyle;

		static SpeedrunTimer()
		{
			updater = new Updater();
			updater.GetLatestVersionAsync();

			timerStyle = new GUIStyle
			{
				fontSize = 16,
				fontStyle = FontStyle.Bold
			};
			timerStyle.normal.textColor = Color.white;

			updateStyle = new GUIStyle
			{
				fontSize = 18,
				fontStyle = FontStyle.Bold
			};
			updateStyle.normal.textColor = Color.white;
		}

		public void Awake()
		{
			instance = this;
		}

		public void Update()
		{
			UpdateInput();

			if (nextAction != null && nextActionFrame == Time.frameCount)
			{
				nextAction();
				nextAction = null;
			}

			if (speedrunEndRealTime != 0 || isGameTimePaused)
				return;

			gameTime += (Time.timeScale != 0) ? (Time.deltaTime / Time.timeScale) : Time.deltaTime;
		}

		void UpdateInput()
		{
			if (!Application.isLoadingLevel && Input.GetKeyUp(KeyCode.R))
			{
				ResetTimer();
				Application.LoadLevel("Level_Menu");
			}

			if (Input.GetKeyDown(KeyCode.H))
				hideTimer = !hideTimer;
		}

		public static void DoActionAt(ActionDel action, int frame)
		{
			nextAction = action;
			nextActionFrame = frame;
		}

		public static void StartTimer()
		{
			if (speedrunStartRealTime != 0)
				return;

			ResetTimer();
			speedrunStartRealTime = Time.realtimeSinceStartup;
			DoActionAt(() => isGameTimePaused = false, Time.frameCount + 1);
		}

		public static void StopTimer()
		{
			if (speedrunStartRealTime == 0)
				return;

			DoActionAt(() => speedrunEndRealTime = Time.realtimeSinceStartup, Time.frameCount + 1);
		}

		public static void StartLoad()
		{
			if (Application.loadedLevelName != "Level_Menu")
				return;

			DoActionAt(() => isGameTimePaused = true, Time.frameCount + 1);
		}

		public static void EndLoad()
		{
			isGameTimePaused = false;
		}

		static void ResetTimer()
		{
			speedrunStartRealTime = 0;
			speedrunEndRealTime = 0;
			gameTime = 0;
			realTime = 0;
			isGameTimePaused = true;
		}

		public void OnGUI()
		{
			if (updater.NeedUpdate)
			{
				var updateText = $"A new Speedrun Timer version is available (v{updater.LatestVersion})";
				var updatePos = new Rect(4, Screen.height - updateStyle.fontSize - 4, Screen.width, Screen.height);
				GUI.Label(updatePos, updateText, updateStyle);
			}

			if (!hideTimer)
			{
				var timerPos = new Rect(4, 0, Screen.width, 100);
				GUI.Label(timerPos, Utils.FormatTime(gameTime), timerStyle);
			}
		}
	}
}
