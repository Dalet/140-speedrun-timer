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

		bool _isGameTimePaused;
		bool _visualFreeze;
		double _gameTime;
		double _realTime;
		Stopwatch _sw_gameTime = new Stopwatch();
		Stopwatch _sw_realTime = new Stopwatch();

		Action _lateUpdateAction;

		public void Awake()
		{
			instance = this;
			gameObject.AddComponent<UI>();

			if (Application.loadedLevel == 5) // level 4
				Unfreeze();
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
				_sw_gameTime.Stop();
				_sw_realTime.Stop();
			});
		}

		public void ResetTimer()
		{
			_sw_gameTime.Reset();
			_sw_realTime.Reset();
			_isGameTimePaused = false;
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
			_visualFreeze = true;
			DoAfterUpdate(() =>
			{
				UpdateVisibleTime();
			});
		}

		public void Unfreeze()
		{
			_visualFreeze = false;
		}
	}
}
