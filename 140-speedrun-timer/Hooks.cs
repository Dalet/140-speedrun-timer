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
			SpeedrunTimer.Instance.StartTimer();
		}

		public void Awake()
		{
			if (SpeedrunTimerLoader.IsLegacyVersion)
				return;

			// timer is frozen at the end of level 3 incase it is the legacy category
			if (Application.loadedLevel == 4 || Application.loadedLevel == 5) // 5 == level 4
				SpeedrunTimer.Instance.Unfreeze();
		}
	}
}
