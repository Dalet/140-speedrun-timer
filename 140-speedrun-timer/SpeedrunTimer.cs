using System;
using System.Diagnostics;
using UnityEngine;

namespace SpeedrunTimerMod
{
	public sealed class SpeedrunTimer : MonoBehaviour
    {
		static SpeedrunTimer instance;
		public static SpeedrunTimer Instance => instance;

		public static bool IsGameTimePaused => instance?.isGameTimePaused ?? false;
		public static bool IsRunning => instance?.sw_realTime.IsRunning ?? false;

		public static double GameTime => instance?.gameTime ?? 0;
		public static double RealTime => instance?.realTime ?? 0;

		bool isGameTimePaused;
		double gameTime;
		double realTime;
		Stopwatch sw_gameTime = new Stopwatch();
		Stopwatch sw_realTime = new Stopwatch();

		Action lateUpdateAction;

		public void Awake()
		{
			instance = this;
			gameObject.AddComponent<UI>();
		}

		public void LateUpdate()
		{
			if (lateUpdateAction != null)
			{
				lateUpdateAction();
				lateUpdateAction = null;
			}

			realTime = sw_realTime.ElapsedSeconds();
			gameTime = sw_gameTime.ElapsedSeconds();
		}

		void DoAfterUpdate(Action action)
		{
			if (lateUpdateAction != null)
				return;

			lateUpdateAction = action;
		}

		public void StartTimer()
		{
			if (IsRunning || realTime > 0 || Application.loadedLevelName != "Level_Menu")
				return;

			DoAfterUpdate(() =>
			{
				ResetTimer();
				sw_gameTime.Start();
				sw_realTime.Start();
			});
		}

		public void StopTimer()
		{
			DoAfterUpdate(() =>
			{
				sw_gameTime.Stop();
				sw_realTime.Stop();
			});
		}

		public void ResetTimer()
		{
			sw_gameTime.Reset();
			sw_realTime.Reset();
			isGameTimePaused = false;
		}

		public void StartLoad()
		{
			if (isGameTimePaused)
				return;

			DoAfterUpdate(() =>
			{
				sw_gameTime.Stop();
				isGameTimePaused = true;
			});
		}

		public void EndLoad()
		{
			if (!isGameTimePaused)
				return;

			DoAfterUpdate(() =>
			{
				sw_gameTime.Start();
				isGameTimePaused = false;
			});
		}
	}
}
