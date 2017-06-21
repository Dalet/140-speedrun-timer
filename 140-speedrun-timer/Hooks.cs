using UnityEngine;

namespace SpeedrunTimerMod
{
	class Hooks : MonoBehaviour
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
			SpeedrunTimer.Instance.CompleteLevel3();
		}

		public static void OnLevel4BossEnd()
		{
			SpeedrunTimer.Instance.CompleteLevel4();
		}

		public static void OnMenuKeyUsed()
		{
			SpeedrunTimer.Instance.StartLoad();
		}

		public static void OnResumeAfterDeath()
		{
			if (Application.loadedLevelName == "Level_Menu")
				SpeedrunTimer.Instance.StartTimer();
		}
	}
}
