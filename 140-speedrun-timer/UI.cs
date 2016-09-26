using System;
using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	internal sealed class UI : MonoBehaviour
	{
		Utils.Label gameTimeLabel;
		Utils.Label realTimeLabel;
		Utils.Label updateLabel;
		Utils.Label debugLabel;
		Utils.Label titleLabel;

		const int BASE_UI_RESOLUTION = 720;

		public void Awake()
		{
			if (string.IsNullOrEmpty(Updater.LatestVersion))
				gameObject.AddComponent<Updater>();

			var color = new Color(235 / 255f, 235 / 255f, 235 / 255f);

			var timerStyle = new GUIStyle
			{
				fontSize = Scale(20),
				fontStyle = FontStyle.Bold,
			};

			gameTimeLabel = new Utils.Label()
			{
				style = timerStyle,
				position = new Rect(Scale(4), 0, Screen.width, Screen.height)
			};

			realTimeLabel = new Utils.Label()
			{
				enabled = false,
				position = new Rect(gameTimeLabel.position.xMin, gameTimeLabel.position.yMin + timerStyle.fontSize,
					gameTimeLabel.position.width, gameTimeLabel.position.height),
				style = timerStyle
			};

			updateLabel = new Utils.Label()
			{
				style = new GUIStyle
				{
					fontSize = Scale(18),
					fontStyle = FontStyle.Bold
				},
			};
			updateLabel.position = new Rect(Scale(4), Screen.height - updateLabel.style.fontSize - Scale(4), Screen.width, Screen.height);

			debugLabel = new Utils.Label()
			{
				enabled = false,
				style = new GUIStyle
				{
					fontSize = Scale(16),
					fontStyle = FontStyle.Bold
				}
			};
			debugLabel.position = new Rect(Scale(4), updateLabel.position.yMin - debugLabel.style.fontSize - Scale(3),
					Screen.width, Screen.height);

			titleLabel = new Utils.Label()
			{
				style = gameTimeLabel.style,
				position = gameTimeLabel.position,
				text = $"Speedrun Timer v{ Utils.FormatVersion(Assembly.GetExecutingAssembly().GetName().Version)}"
#if DEBUG
					+ " (debug)"
#endif
			};

			timerStyle.normal.textColor = debugLabel.style.normal.textColor
				= updateLabel.style.normal.textColor = color;

			ReadSettings();
			titleLabel.enabled = !gameTimeLabel.enabled;
		}

		public void OnGUI()
		{
			var rt = SpeedrunTimer.RealTime;
			var gt = SpeedrunTimer.GameTime;

			if (!gameTimeLabel.enabled && Time.realtimeSinceStartup < 10)
				titleLabel.OnGUI();

			gameTimeLabel.OnGUI(Utils.FormatTime(gt));
			if (gameTimeLabel.enabled)
				realTimeLabel.OnGUI(Utils.FormatTime(rt));

			debugLabel.OnGUI($"Level {Application.loadedLevel} \"{Application.loadedLevelName}\" | .NET: {Environment.Version} | Unity: {Application.unityVersion}");

			if (Updater.NeedUpdate)
				updateLabel.OnGUI($"A new Speedrun Timer version is available (v{Updater.LatestVersion})");
		}

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				realTimeLabel.enabled = !realTimeLabel.enabled || !gameTimeLabel.enabled;
				gameTimeLabel.enabled = true;
			}

			if (Input.GetKeyDown(KeyCode.F2))
			{
				gameTimeLabel.enabled = !gameTimeLabel.enabled;
				titleLabel.enabled = false;
			}

			if (Input.GetKeyDown(KeyCode.F3))
				debugLabel.enabled = !debugLabel.enabled;
		}

		void ReadSettings()
		{
			gameTimeLabel.enabled = Utils.PlayerPrefsGetBool("ShowTimer", gameTimeLabel.enabled);
			realTimeLabel.enabled = Utils.PlayerPrefsGetBool("ShowRealTime", realTimeLabel.enabled);
		}

		public void OnApplicationQuit()
		{
			Utils.PlayerPrefsSetBool("ShowTimer", gameTimeLabel.enabled);
			Utils.PlayerPrefsSetBool("ShowRealTime", realTimeLabel.enabled);
			PlayerPrefs.Save();
		}

		public static int Scale(int pixels)
		{
			if (Screen.height <= BASE_UI_RESOLUTION)
				return pixels;
			else
				return (int)Math.Round(pixels * (Screen.height / (float)BASE_UI_RESOLUTION));
		}
	}
}
