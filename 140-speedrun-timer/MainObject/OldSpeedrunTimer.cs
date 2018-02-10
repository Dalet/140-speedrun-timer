using SpeedrunTimerMod.Logging;
using System;
using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class OldSpeedrunTimer : MonoBehaviour
	{
		public static OldSpeedrunTimer Instance { get; private set; }

		public bool IsGameTimePaused => _stopwatch.IsPaused;
		public bool IsRunning => _stopwatch.IsStarted;

		public TimeSpan GameTime { get; private set; }
		public TimeSpan RealTime { get; private set; }

		public RunLog RunLog { get; private set; }

		event Action LateUpdateActions;
		int _lastLastUpdateFrame;

		SpeedrunStopwatch _stopwatch;
		bool _visualFreeze;

		void Awake()
		{
			Instance = this;
			_stopwatch = new SpeedrunStopwatch();
			RunLog = new RunLog();
		}

		void OnDestroy()
		{
			Instance = null;
		}

		void OnLevelWasLoaded(int index)
		{
			// end of the 'Level 3 -> Menu' load
			if (Application.loadedLevelName == "Level_Menu")
				EndLoad();
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
			RealTime = _stopwatch.RealTime;
			GameTime = _stopwatch.GameTime;
		}

		public void LevelStart(int level)
		{
			DoAfterUpdate(() =>
			{
				var time = new SpeedrunTime(_stopwatch.RealTime, _stopwatch.GameTime);
				RunLog.LevelStart(level, time);
			});
		}

		public void CompleteLevel(int level)
		{
			if (level == 3 || level == 4 || ModLoader.Settings.ILMode)
				Freeze();

			DoAfterUpdate(() =>
			{
				var time = new SpeedrunTime(_stopwatch.RealTime, _stopwatch.GameTime);
				RunLog.CompleteLevel(level, time);

				if (level == 2)
					LogLevel2();
			});
		}

		void LogLevel2()
		{
			var writer = new RunLogCsvWriter(SpeedrunTimer.Instance.RunLog, RunLog)
			{
				Level2 = true
			};
			StartCoroutine(writer.WriteToLogAsyncOnFrameEnd());
		}

		public void StartTimer()
		{
			DoAfterUpdate(() =>
			{
				if (IsRunning)
					return;

				ResetTimer();
				_stopwatch.Start();
				RunLog.StartDate = _stopwatch.StartDate;
			});
		}

		public void ResetTimer()
		{
			_stopwatch.Reset();
			_visualFreeze = false;
			RunLog = new RunLog();
		}

		public void StartLoad()
		{
			DoAfterUpdate(() =>
			{
				if (!IsRunning || IsGameTimePaused)
					return;

				_stopwatch.Pause();
			});
		}

		public void EndLoad()
		{
			DoAfterUpdate(() =>
			{
				if (!IsRunning || !IsGameTimePaused)
					return;

				_stopwatch.Resume();
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
