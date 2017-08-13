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
			var color = new Color(235 / 255f, 235 / 255f, 235 / 255f);

			var timerStyle = new GUIStyle
			{
				fontStyle = FontStyle.Bold
			};
			var timerFontSize = 20;

			_gameTimeLabel = new Utils.Label()
			{
				style = timerStyle,
				fontSize = timerFontSize,
				positionDelegate = () => new Rect(Scale(4), 0, Screen.width, Screen.height)
			};

			_realTimeLabel = new Utils.Label()
			{
				enabled = false,
				positionDelegate = () => new Rect(_gameTimeLabel.Position.xMin, _gameTimeLabel.Position.yMin + timerStyle.fontSize,
					_gameTimeLabel.Position.width, _gameTimeLabel.Position.height),
				style = timerStyle,
				fontSize = timerFontSize
			};

			_updateLabel = new Utils.Label()
			{
				positionDelegate = () => new Rect(Scale(4), Screen.height - _updateLabel.style.fontSize - Scale(4), Screen.width, Screen.height),
				fontSize = 18,
				style = new GUIStyle
				{
					fontStyle = FontStyle.Bold
				},
			};

			_debugLabel = new Utils.Label()
			{
				enabled = false,
				positionDelegate = () => new Rect(Scale(4), _updateLabel.Position.yMin - _debugLabel.style.fontSize * 5 - Scale(3),
					Screen.width, Screen.height),
				fontSize = 16,
				style = new GUIStyle
				{
					fontStyle = FontStyle.Bold
				}
			};

			_titleLabel = new Utils.Label()
			{
				style = _gameTimeLabel.style,
				fontSize = _gameTimeLabel.fontSize,
				positionDelegate = _gameTimeLabel.positionDelegate,
				text = $"Speedrun Timer v{ Utils.FormatVersion(Assembly.GetExecutingAssembly().GetName().Version)}"
#if DEBUG
					+ " (debug)"
#elif PRE_RELEASE
					+ " (pre-release)"
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
					$"Checkpoint: {currentCheckpoint + 1} | Beat: {Misc.BeatDbgStr} | Pos: ({PadPosition(pos.x)}, {PadPosition(pos.y)})\n"
					+ $"Frame: {Time.renderedFrameCount} | IsRunning: {isRunning} | IsGameTimePaused: {gtPaused} | LiveSplitSyncEnabled: {livesplitSyncEnabled}, TryingToConnect: {liveplitSyncConnecting}\n"
					+ $"Level {Application.loadedLevel} \"{Application.loadedLevelName}\" | .NET: {Environment.Version} | Unity: {Application.unityVersion} | Legacy: {ModLoader.IsLegacyVersion}"
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
			return (int)Math.Round(pixels * (Screen.height / (float)BASE_UI_RESOLUTION));
		}
	}
}
