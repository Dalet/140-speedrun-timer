using System;
using UnityEngine;

namespace SpeedrunTimerMod
{
	internal sealed class UI : MonoBehaviour
	{
		Utils.Label gameTimeLabel;
		Utils.Label realTimeLabel;
		Utils.Label updateLabel;
		Utils.Label debugLabel;

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

			timerStyle.normal.textColor = debugLabel.style.normal.textColor
				= updateLabel.style.normal.textColor = color;
		}

		public void OnGUI()
		{
			var rt = SpeedrunTimer.RealTime;
			var gt = SpeedrunTimer.GameTime;

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
				gameTimeLabel.enabled = !gameTimeLabel.enabled;

			if (Input.GetKeyDown(KeyCode.F3))
				debugLabel.enabled = !debugLabel.enabled;
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
