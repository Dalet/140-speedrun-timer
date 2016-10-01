using System;
using System.Collections;
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

		Action endOfFrameAction;

		public void Awake()
		{
			instance = this;
			gameObject.AddComponent<UI>();
		}

		public void Update()
		{
			realTime = sw_realTime.ElapsedSeconds();
			gameTime = sw_gameTime.ElapsedSeconds();

			if (endOfFrameAction != null)
				StartCoroutine(WaitForEndOfFrame());
		}

		IEnumerator WaitForEndOfFrame()
		{
			if (endOfFrameAction == null)
				yield return null;

			yield return new WaitForEndOfFrame();
			endOfFrameAction();
			endOfFrameAction = null;
		}

		void DoAtEndOfFrame(Action action)
		{
			if (endOfFrameAction != null)
				return;

			endOfFrameAction = action;
		}

		public void StartTimer()
		{
			if (IsRunning || realTime > 0 || Application.loadedLevelName != "Level_Menu")
				return;

			DoAtEndOfFrame(() =>
			{
				ResetTimer();
				sw_gameTime.Start();
				sw_realTime.Start();
			});
		}

		public void StopTimer()
		{
			DoAtEndOfFrame(() =>
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

			DoAtEndOfFrame(() =>
			{
				sw_gameTime.Stop();
				isGameTimePaused = true;
			});
		}

		public void EndLoad()
		{
			if (!isGameTimePaused)
				return;

			DoAtEndOfFrame(() =>
			{
				sw_gameTime.Start();
				isGameTimePaused = false;
			});
		}
	}
}
