using System;
using System.Diagnostics;
using UnityEngine;

namespace SpeedrunTimerMod
{
	public sealed class SpeedrunTimer : MonoBehaviour
    {
		static SpeedrunTimer instance;
		public static SpeedrunTimer Instance => instance;

		public bool IsGameTimePaused => _isGameTimePaused;
		public bool IsRunning => _sw_realTime.IsRunning;

		public double GameTime => _gameTime;
		public double RealTime => _realTime;

		bool _livesplitSyncEnabled;
		public bool LiveSplitSyncEnabled
		{
			get { return _livesplitSyncEnabled; }
			set
			{
				if (_livesplitSyncEnabled == value)
					return;

				_livesplitSyncEnabled = value;
				if (_livesplitSyncEnabled)
				{
					LiveSplitSync.ConnectAsync();
				}
				else if (LiveSplitSync != null)
				{
					LiveSplitSync.GracefulDisconnect();
				}
				Utils.PlayerPrefsSetBool("LiveSplitServerSync", _livesplitSyncEnabled);
			}
		}

		public LiveSplitSync LiveSplitSync { get; private set; }

		bool _isGameTimePaused;
		double _gameTime;
		double _realTime;
		Stopwatch _sw_gameTime = new Stopwatch();
		Stopwatch _sw_realTime = new Stopwatch();
		bool _visualFreeze;

		event Action LateUpdateActions;

		void Awake()
		{
			instance = this;
			gameObject.AddComponent<UI>();
			LiveSplitSync = new LiveSplitSync();
			LiveSplitSync.Connected += LiveSplitSync_OnConnected;
			LiveSplitSyncEnabled = Utils.PlayerPrefsGetBool("LiveSplitServerSync", false);
		}

		void LiveSplitSync_OnConnected(object sender, EventArgs e)
		{
			if (!IsRunning)
				LiveSplitSync.Reset();
		}

		void OnDestroy()
		{
			LiveSplitSync?.GracefulDispose();
		}

		void OnLevelWasLoaded(int index)
		{
			// unfreeze the timer on level loads
			if (Application.loadedLevelName != "Level_Menu")
				Unfreeze();

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

			if (!_visualFreeze)
			{
				UpdateVisibleTime();
			}
		}

		void UpdateVisibleTime()
		{
			var gameTimeTs = _sw_gameTime.Elapsed;
			_realTime = _sw_realTime.Elapsed.TotalSeconds;
			_gameTime = gameTimeTs.TotalSeconds;

			if (LiveSplitSyncEnabled)
				LiveSplitSync.UpdateTime(gameTimeTs, _visualFreeze); // force the update if frozen
		}

		void DoAfterUpdate(Action action)
		{
			LateUpdateActions += action;
		}

		public void CompleteLevel3()
		{
			LevelCompleted();
			StartLoad();
		}

		public void CompleteLevel4()
		{
			LevelCompleted();
		}

		void LevelCompleted()
		{
			Freeze();
			if (LiveSplitSyncEnabled)
			{
				DoAfterUpdate(() =>
				{
					LiveSplitSync.Split();
				});
			}
		}

		public void StartTimer()
		{
			if (IsRunning || _realTime > 0 || Application.loadedLevelName != "Level_Menu")
				return;

			DoAfterUpdate(() =>
			{
				ResetTimer();
				_sw_gameTime.Start();
				_sw_realTime.Start();

				if (LiveSplitSyncEnabled)
					LiveSplitSync.Start();
			});
		}

		public void StopTimer()
		{
			DoAfterUpdate(() =>
			{
				_visualFreeze = false;
				_sw_gameTime.Stop();
				_sw_realTime.Stop();
			});
		}

		public void ResetTimer()
		{
			_sw_gameTime.Reset();
			_sw_realTime.Reset();
			_isGameTimePaused = false;
			_visualFreeze = false;

			if (LiveSplitSyncEnabled)
				LiveSplitSync.Reset();
		}

		public void StartLoad()
		{
			if (!IsRunning || _isGameTimePaused)
				return;

			DoAfterUpdate(() =>
			{
				_sw_gameTime.Stop();
				_isGameTimePaused = true;
			});
		}

		public void EndLoad()
		{
			if (!IsRunning || !_isGameTimePaused)
				return;

			DoAfterUpdate(() =>
			{
				_sw_gameTime.Start();
				_isGameTimePaused = false;
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
		}
	}
}
