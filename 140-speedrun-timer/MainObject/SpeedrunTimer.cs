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

		public LiveSplitSync LiveSplitSync { get; private set; }

		event Action LateUpdateActions;
		BeatTimer _beatTimer;
		BeatTimerController _beatController;
		bool _visualFreeze;
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
					LiveSplitSync.ConnectAsync();
				else
					LiveSplitSync.GracefulDisconnect();
			}
		}

		void Awake()
		{
			Instance = this;
			_beatTimer = new BeatTimer(140);
			_beatController = gameObject.AddComponent<BeatTimerController>();
			_beatController.BeatTimer = _beatTimer;
			LiveSplitSync = new LiveSplitSync()
			{
				AlwaysPauseGameTime = true
			};
			LiveSplitSync.Connected += LiveSplitSync_OnConnected;
		}

		void LiveSplitSync_OnConnected(object sender, EventArgs e)
		{
			if (!IsRunning)
				LiveSplitSync.Reset();
		}

		void OnDestroy()
		{
			LiveSplitSync.GracefulDispose();
			Instance = null;
		}

		void OnApplicationPause(bool pause)
		{
			if (pause)
				_beatTimer.PauseInterpolation();
			else
				_beatTimer.ResumeInterpolation();
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

			RealTime = _beatTimer.InterpolatedRealTime.TotalSeconds;
			GameTime = gameTimeTs.TotalSeconds;

			if (LiveSplitSyncEnabled && IsRunning)
			{
				LiveSplitSync.UpdateTime(gameTimeTs, _visualFreeze); // force the update if frozen
			}
		}

		public void CompleteLevel(int level)
		{
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
			if (!IsRunning)
				return;

			// if the split is on beat, the beatTimer might not have ticked yet (onBeat eventhandler not called yet)
			// so delay until LateUpdate
			DoAfterUpdate(() =>
			{
				if (IsGameTimePaused)
				{
					millisecondsOffset = 0;
					quarterbeatsOffset = 0;
				}

				var offset = new BeatTime(_beatTimer.Bpm, quarterbeatsOffset, millisecondsOffset);
				var splitBeatTime = _beatTimer.Time + offset;
				var timespan = splitBeatTime.TimeSpan;
				var timeStr = Utils.FormatTime(timespan.TotalSeconds, 3);

				var debugStr = $"Split at {timeStr} / {splitBeatTime}";
				if (millisecondsOffset != 0 || quarterbeatsOffset != 0)
					debugStr += $" ({offset} offset)";
				Debug.Log(debugStr + "\n" + DebugBeatListener.DebugStr);

				if (LiveSplitSyncEnabled)
					LiveSplitSync.Split(timespan);
			});
		}

		public void Unsplit()
		{
			Unfreeze();
			DoAfterUpdate(() =>
			{
				Debug.Log("Unsplit");
				if (LiveSplitSyncEnabled)
					LiveSplitSync.Unsplit();
			});
		}

		/// <summary>
		/// Offsets relative to the last beat
		/// </summary>
		/// <param name="millisecondsOffset"></param>
		/// <param name="quarterBeatsOffset"></param>
		public void StartTimer(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			if (IsRunning)
				return;

			DoAfterUpdate(() =>
			{
				ResetTimer();
				_beatTimer.StartTimer(millisecondsOffset, quarterBeatsOffset);

				if (LiveSplitSyncEnabled)
					LiveSplitSync.Start();
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
			_beatTimer.ResetTimer();
			_visualFreeze = false;

			if (LiveSplitSyncEnabled)
				LiveSplitSync.Reset();
		}

		/// <summary>
		/// Offsets relative to the last beat
		/// </summary>
		/// <param name="millisecondsOffset"></param>
		/// <param name="quarterBeatsOffset"></param>
		public void StartLoad(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			if (!IsRunning || IsGameTimePaused)
				return;

			DoAfterUpdate(() =>
			{
				_beatTimer.PauseTimer(millisecondsOffset, quarterBeatsOffset);
			});
		}

		public void EndLoad()
		{
			if (!IsRunning || !IsGameTimePaused)
				return;

			DoAfterUpdate(() =>
			{
				_beatTimer.ResumeTimer();
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