using System;
using System.Linq;
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
		MyCharacterController player;

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
			debugLabel.position = new Rect(Scale(4), updateLabel.position.yMin - debugLabel.style.fontSize * 5 - Scale(3),
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

			if (!gameTimeLabel.enabled && Time.realtimeSinceStartup < 10)
				titleLabel.OnGUI();

			if (gameTimeLabel.enabled)
			{
				gameTimeLabel.OnGUI(Utils.FormatTime(SpeedrunTimer.GameTime));
				realTimeLabel.OnGUI(Utils.FormatTime(SpeedrunTimer.RealTime));
			}

			if (debugLabel.enabled)
			{
				var pos = player.transform.position;
				var currentCheckpoint = Globals.levelsManager.GetCurrentCheckPoint();
				debugLabel.OnGUI(
					$"Checkpoint: {currentCheckpoint + 1}/{Cheats.Savepoints.Length} | Beat: {Misc.BeatDbgStr} | Pos: ({PadPosition(pos.x)}, {PadPosition(pos.y)})\n"
					+ $"Frame: {Time.renderedFrameCount} | IsRunning: {SpeedrunTimer.IsRunning} | IsGameTimePaused: {SpeedrunTimer.IsGameTimePaused}\n"
					+ $"Level {Application.loadedLevel} \"{Application.loadedLevelName}\" | .NET: {Environment.Version} | Unity: {Application.unityVersion}"
				);
			}

			if (Updater.NeedUpdate)
				updateLabel.OnGUI($"A new Speedrun Timer version is available (v{Updater.LatestVersion})");
		}

		static string PadPosition(float p)
		{
			var str = p.ToString();
			var padding = 7 - str.Count(c => char.IsDigit(c));
			if (padding > 0)
			{
				if (str.IndexOf('.') < 0)
					str += '.';
				str = str.PadRight(str.Length + padding, '0');
			}
			return str;
		}

		public void Update()
		{
			player = Globals.player.GetComponent<MyCharacterController>();

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
