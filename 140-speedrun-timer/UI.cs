using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	internal sealed class UI : MonoBehaviour
	{
		Utils.Label _gameTimeLabel;
		Utils.Label _realTimeLabel;
		Utils.Label _updateLabel;
		Utils.Label _debugLabel;
		Utils.Label _titleLabel;
		MyCharacterController _player;

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

			_gameTimeLabel = new Utils.Label()
			{
				style = timerStyle,
				position = new Rect(Scale(4), 0, Screen.width, Screen.height)
			};

			_realTimeLabel = new Utils.Label()
			{
				enabled = false,
				position = new Rect(_gameTimeLabel.position.xMin, _gameTimeLabel.position.yMin + timerStyle.fontSize,
					_gameTimeLabel.position.width, _gameTimeLabel.position.height),
				style = timerStyle
			};

			_updateLabel = new Utils.Label()
			{
				style = new GUIStyle
				{
					fontSize = Scale(18),
					fontStyle = FontStyle.Bold
				},
			};
			_updateLabel.position = new Rect(Scale(4), Screen.height - _updateLabel.style.fontSize - Scale(4), Screen.width, Screen.height);

			_debugLabel = new Utils.Label()
			{
				enabled = false,
				style = new GUIStyle
				{
					fontSize = Scale(16),
					fontStyle = FontStyle.Bold
				}
			};
			_debugLabel.position = new Rect(Scale(4), _updateLabel.position.yMin - _debugLabel.style.fontSize * 5 - Scale(3),
					Screen.width, Screen.height);

			_titleLabel = new Utils.Label()
			{
				style = _gameTimeLabel.style,
				position = _gameTimeLabel.position,
				text = $"Speedrun Timer v{ Utils.FormatVersion(Assembly.GetExecutingAssembly().GetName().Version)}"
#if DEBUG
					+ " (debug)"
#endif
			};

			timerStyle.normal.textColor = _debugLabel.style.normal.textColor
				= _updateLabel.style.normal.textColor = color;

			ReadSettings();
			_titleLabel.enabled = !_gameTimeLabel.enabled;
		}

		public void OnGUI()
		{

			if (!_gameTimeLabel.enabled && Time.realtimeSinceStartup < 10)
				_titleLabel.OnGUI();

			if (_gameTimeLabel.enabled)
			{
				var livesplitConnected = SpeedrunTimer.Instance.LiveSplitSync?.IsConnected ?? false;
				var gtSuffix = livesplitConnected ? "•" : "";
				_gameTimeLabel.OnGUI(Utils.FormatTime(SpeedrunTimer.Instance.GameTime) + gtSuffix);
				_realTimeLabel.OnGUI(Utils.FormatTime(SpeedrunTimer.Instance.RealTime));
			}

			if (_debugLabel.enabled)
			{
				var pos = _player.transform.position;
				var currentCheckpoint = Globals.levelsManager.GetCurrentCheckPoint();

				var isRunning = SpeedrunTimer.Instance.IsRunning;
				var gtPaused = SpeedrunTimer.Instance.IsGameTimePaused;
				var livesplitSyncEnabled = SpeedrunTimer.Instance.LiveSplitSyncEnabled;
				var liveplitSyncConnecting = SpeedrunTimer.Instance.LiveSplitSync?.IsConnecting ?? false;


				_debugLabel.OnGUI(
					$"Checkpoint: {currentCheckpoint + 1}/{Cheats.Savepoints.Length} | Beat: {Misc.BeatDbgStr} | Pos: ({PadPosition(pos.x)}, {PadPosition(pos.y)})\n"
					+ $"Frame: {Time.renderedFrameCount} | IsRunning: {isRunning} | IsGameTimePaused: {gtPaused} | LiveSplitSyncEnabled: {livesplitSyncEnabled}, TryingToConnect: {liveplitSyncConnecting}\n"
					+ $"Level {Application.loadedLevel} \"{Application.loadedLevelName}\" | .NET: {Environment.Version} | Unity: {Application.unityVersion} | Legacy: {SpeedrunTimerLoader.IsLegacyVersion}"
				);
			}

			if (Updater.NeedUpdate)
				_updateLabel.OnGUI($"A new Speedrun Timer version is available (v{Updater.LatestVersion})");
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
			_player = Globals.player.GetComponent<MyCharacterController>();

			if (Input.GetKeyDown(KeyCode.F1))
			{
				_realTimeLabel.enabled = !_realTimeLabel.enabled || !_gameTimeLabel.enabled;
				_gameTimeLabel.enabled = true;
			}

			if (Input.GetKeyDown(KeyCode.F2))
			{
				_gameTimeLabel.enabled = !_gameTimeLabel.enabled;
				_titleLabel.enabled = false;
			}

			if (Input.GetKeyDown(KeyCode.F3))
				_debugLabel.enabled = !_debugLabel.enabled;
		}

		void ReadSettings()
		{
			_gameTimeLabel.enabled = Utils.PlayerPrefsGetBool("ShowTimer", _gameTimeLabel.enabled);
			_realTimeLabel.enabled = Utils.PlayerPrefsGetBool("ShowRealTime", _realTimeLabel.enabled);
		}

		public void OnApplicationQuit()
		{
			Utils.PlayerPrefsSetBool("ShowTimer", _gameTimeLabel.enabled);
			Utils.PlayerPrefsSetBool("ShowRealTime", _realTimeLabel.enabled);
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
