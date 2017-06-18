using System;
using System.Diagnostics;
using UnityEngine;

namespace SpeedrunTimerMod
{
	public sealed class SpeedrunTimer : MonoBehaviour
    {
		static SpeedrunTimer instance;
		public static SpeedrunTimer Instance => instance;

		public static bool IsGameTimePaused => instance?._isGameTimePaused ?? false;
		public static bool IsRunning => instance?._sw_realTime.IsRunning ?? false;

		public static double GameTime => instance?._gameTime ?? 0;
		public static double RealTime => instance?._realTime ?? 0;

		public bool Level3Completed { get; set; }
		public bool Level4Completed { get; set; }

		bool _isGameTimePaused;
		double _gameTime;
		double _realTime;
		Stopwatch _sw_gameTime = new Stopwatch();
		Stopwatch _sw_realTime = new Stopwatch();
		bool _visualFreeze;

		Action _lateUpdateAction;

		public void Awake()
		{
			instance = this;
			gameObject.AddComponent<UI>();
		}

		public void LateUpdate()
		{
			if (_lateUpdateAction != null)
			{
				_lateUpdateAction();
				_lateUpdateAction = null;
			}

			if (!_visualFreeze)
			{
				UpdateVisibleTime();
			}
		}

		void UpdateVisibleTime()
		{
			_realTime = _sw_realTime.ElapsedSeconds();
			_gameTime = _sw_gameTime.ElapsedSeconds();
		}

		void DoAfterUpdate(Action action)
		{
			if (_lateUpdateAction != null)
				return;

			_lateUpdateAction = action;
		}

		public void CompleteLevel3()
		{
			Level3Completed = true;
			LevelCompleted();
		}

		public void CompleteLevel4()
		{
			Level4Completed = true;
			LevelCompleted();
		}

		void LevelCompleted()
		{
			if (SpeedrunTimerLoader.IsLegacyVersion)
			{
				StopTimer();
			}
			else if (Level3Completed && Level4Completed)
			{
				StopTimer();
			}
			else
			{
				Freeze();
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
			Level3Completed = Level4Completed = false;
		}

		public void StartLoad()
		{
			if (_isGameTimePaused)
				return;

			DoAfterUpdate(() =>
			{
				_sw_gameTime.Stop();
				_isGameTimePaused = true;
			});
		}

		public void EndLoad()
		{
			if (!_isGameTimePaused)
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
