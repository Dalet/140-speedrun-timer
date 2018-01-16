using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SpeedrunTimerMod
{
	/// <summary>
	/// The game's dll is modified to call these hooks
	/// </summary>
	static class Hooks
	{
		public static event Action OnPlayerResumeControl;

		public static void OnResumeAfterDeath()
		{
			/*
			Debug.Log("OnResumeAfterDeath: " + PersistentBeatListener.DebugStr);

			if (SpeedrunTimer.Instance == null)
				return;

			if (!SpeedrunTimer.Instance.IsRunning && Application.loadedLevelName == "Level_Menu")
			{
				var playerOverride = ModLoader.LevelObject.GetComponent<PlayerControlOverride>();
				playerOverride.SetCallback(() => SpeedrunTimer.Instance?.StartTimer());
				playerOverride.HoldUntilBeat(Utils.GetSimilarQuarterBeats(4));
			}
			*/
		}

		public static void PlayerResumeControl()
		{
			OnPlayerResumeControl?.Invoke();
		}
	}
}
