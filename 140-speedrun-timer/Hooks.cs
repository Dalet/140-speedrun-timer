using UnityEngine;

namespace SpeedrunTimerMod
{
	static class Hooks
	{
		public static void OnPlayerFixedUpdate(bool logicPaused, bool controlPaused, Vector3 moveDirection)
		{
			if (!logicPaused && controlPaused && moveDirection.y == 0f)
			{
				SpeedrunTimer.Instance?.EndLoad();
			}
		}

		public static void OnLevel3BossEnd()
		{
			SpeedrunTimer.Instance.Freeze();
		}

		public static void OnLevel4BossEnd()
		{
			SpeedrunTimer.Instance.StopTimer();
		}

		public static void OnMenuKeyUsed()
		{
			SpeedrunTimer.Instance.StartLoad();
		}

		public static void OnResumeAfterDeath()
		{
			SpeedrunTimer.Instance.StartTimer();
		}
	}
}
