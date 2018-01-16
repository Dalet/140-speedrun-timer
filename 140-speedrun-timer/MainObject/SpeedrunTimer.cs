using SpeedrunTimerMod.BeatTiming;
using SpeedrunTimerMod.LiveSplit;
using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SpeedrunTimerMod
{
	public sealed class SpeedrunTimer : MonoBehaviour
    {
		public static SpeedrunTimer Instance { get; private set; }

		public bool IsGameTimePaused => _beatTimer.IsPaused;
		public bool IsRunning => _beatTimer.IsStarted;

		public double GameTime { get; private set; }
		public double RealTime { get; private set; }

		public bool LiveSplitConnected => _liveSplitSync?.IsConnected ?? false;
		public bool LiveSplitConnecting => _liveSplitSync?.IsConnecting ?? false;

		event Action LateUpdateActions;

		BeatTimer _beatTimer;
		BeatTimerController _beatController;
		SpeedrunStopwatch _speedrunStopwatch;
		bool _visualFreeze;

		LiveSplitSync _liveSplitSync;
		bool _livesplitSyncEnabled;

		int _lastLastUpdateFrame;

		public bool LiveSplitSyncEnabled
		{
			get { return _livesplitSyncEnabled; }
			set
			{
				if (_livesplitSyncEnabled == value)
					return;

				_livesplitSyncEnabled = value;
				if (_livesplitSyncEnabled)
					_liveSplitSync.ConnectAsync();
				else
					_liveSplitSync.GracefulDisconnect();
			}
		}

		void Awake()
		{
			Instance = this;
			_beatTimer = new BeatTimer(140);
			_beatController = gameObject.AddComponent<BeatTimerController>();
			_beatController.BeatTimer = _beatTimer;
			_liveSplitSync = new LiveSplitSync()
			{
				AlwaysPauseGameTime = true
			};
			_liveSplitSync.Connected += LiveSplitSync_OnConnected;
			_speedrunStopwatch = new SpeedrunStopwatch();
		}

		void LiveSplitSync_OnConnected(object sender, EventArgs e)
		{
			if (!IsRunning)
				_liveSplitSync.Reset();
		}

		void OnDestroy()
		{
			_liveSplitSync.GracefulDispose();
			Instance = null;
		}

		void LateUpdate()
		{
			if (LateUpdateActions != null)
			{
				LateUpdateActions();
				LateUpdateActions = null;
			}
			_lastLastUpdateFrame = Time.frameCount;

			if (!_visualFreeze)
			{
				UpdateVisibleTime();
			}
		}

		void DoAfterUpdate(Action action)
		{
			if (Time.frameCount == _lastLastUpdateFrame)
			{
				// if LateUpdate already happened on this frame, do it now
				action();
			}
			else
			{
				LateUpdateActions += action;
			}
		}

		void UpdateVisibleTime()
		{
			var gameTimeTs = _visualFreeze
				? _beatTimer.Time.TimeSpan
				: _beatTimer.InterpolatedTime;

			RealTime = _speedrunStopwatch.RealTime.TotalSeconds;
			GameTime = gameTimeTs.TotalSeconds;

			if (LiveSplitSyncEnabled && IsRunning)
			{
				_liveSplitSync.UpdateTime(gameTimeTs, _visualFreeze); // force the update if frozen
			}
		}

		public void CompleteLevel(int level)
		{
			if (level == 3 || level == 4 || ModLoader.Settings.ILMode)
				Freeze();

			Split();
		}

		/// <summary>
		/// Offsets relative to the last beat
		/// </summary>
		/// <param name="millisecondsOffset"></param>
		/// <param name="quarterBeatsOffset"></param>
		public void Split(int millisecondsOffset = 0, int quarterbeatsOffset = 0)
		{
			// if the split is on beat, the beatTimer might not have ticked yet (onBeat eventhandler not called yet)
			// so delay until LateUpdate
			DoAfterUpdate(() =>
			{
				if (!IsRunning)
					return;

				if (IsGameTimePaused)
				{
					millisecondsOffset = 0;
					quarterbeatsOffset = 0;
				}

				var offset = new BeatTime(_beatTimer.Bpm, quarterbeatsOffset, millisecondsOffset);
				var splitBeatTime = _beatTimer.Time + offset;
				var timeStr = Utils.FormatTime(splitBeatTime.TimeSpan, 3);

				var stopwatchTime = _speedrunStopwatch.GameTime + offset.TimeSpan;
				var stopwatchTimeStr = Utils.FormatTime(stopwatchTime, 3);

				var splitTimeStr = $"Split at {timeStr} / {splitBeatTime}";
				var offsetStr = millisecondsOffset != 0 || quarterbeatsOffset != 0
					? $" ({offset} offset)"
					: string.Empty;
				Debug.Log($"{splitTimeStr}{offsetStr}\n(stopwatch: {stopwatchTimeStr})\n"
					+ DebugBeatListener.DebugStr);

				if (LiveSplitSyncEnabled)
					_liveSplitSync.Split(splitBeatTime.TimeSpan);
			});
		}

		public void Unsplit()
		{
			Unfreeze();
			DoAfterUpdate(() =>
			{
				Debug.Log("Unsplit");
				if (LiveSplitSyncEnabled)
					_liveSplitSync.Unsplit();
			});
		}

		/// <summary>
		/// Offsets relative to the last beat
		/// </summary>
		/// <param name="millisecondsOffset"></param>
		/// <param name="quarterBeatsOffset"></param>
		public void StartTimer(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			var beatTime = new BeatTime(_beatTimer.Bpm, quarterBeatsOffset, millisecondsOffset);
			DoAfterUpdate(() =>
			{
				if (IsRunning)
					return;

				ResetTimer();
				_speedrunStopwatch.Start((int)beatTime.Milliseconds);
				_beatTimer.StartTimer(millisecondsOffset, quarterBeatsOffset);

				if (LiveSplitSyncEnabled)
					_liveSplitSync.Start();
			});
		}

		/*public void StopTimer()
		{
			DoAfterUpdate(() =>
			{
				_visualFreeze = false;
				_beatTimer.PauseTimer();
			});
		}*/

		public void ResetTimer()
		{
			_speedrunStopwatch.Reset();
			_beatTimer.ResetTimer();
			_visualFreeze = false;

			if (LiveSplitSyncEnabled)
				_liveSplitSync.Reset();
		}

		/// <summary>
		/// Offsets relative to the last beat
		/// </summary>
		/// <param name="millisecondsOffset"></param>
		/// <param name="quarterBeatsOffset"></param>
		public void StartLoad(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			var beatTimeOffset = new BeatTime(_beatTimer.Bpm, quarterBeatsOffset, millisecondsOffset);
			DoAfterUpdate(() =>
			{
				if (!IsRunning || IsGameTimePaused)
					return;

				_speedrunStopwatch.Pause((int)beatTimeOffset.Milliseconds);
				_beatTimer.PauseTimer(millisecondsOffset, quarterBeatsOffset);
			});
		}

		public void EndLoad(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			var beatTimeOffset = new BeatTime(_beatTimer.Bpm, quarterBeatsOffset, millisecondsOffset);
			DoAfterUpdate(() =>
			{
				if (!IsRunning || !IsGameTimePaused)
					return;

				_speedrunStopwatch.Resume((int)beatTimeOffset.Milliseconds);
				_beatTimer.ResumeTimer(millisecondsOffset, quarterBeatsOffset);
			});
		}

		public void Freeze()
		{
			DoAfterUpdate(() =>
			{
				_visualFreeze = true;
				UpdateVisibleTime();
			});
		}

		public void Unfreeze()
		{
			_visualFreeze = false;
			DoAfterUpdate(() =>
			{
				UpdateVisibleTime();
			});
		}
	}
}
