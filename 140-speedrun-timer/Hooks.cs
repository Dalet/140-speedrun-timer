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
	}
}
