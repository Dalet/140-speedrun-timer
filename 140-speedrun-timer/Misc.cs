using UnityEngine;

namespace SpeedrunTimerMod
{
	internal sealed class Misc : MonoBehaviour
	{
		float resetTimer;
		float resetHits;

		public void Update()
		{
			if (!Application.isLoadingLevel)
			{
				if (resetHits == 1)
					resetTimer += Time.deltaTime;

				if (resetTimer > 0.5f)
					resetHits = 0;

				if (Input.GetKeyDown(KeyCode.R))
				{
					if (resetHits == 0)
						resetTimer = 0;

					resetHits++;
					if (resetHits > 1 && resetTimer < 0.5f)
					{
						resetHits = 0;
						SpeedrunTimer.Instance.ResetTimer();
						Application.LoadLevel("Level_Menu");
					}
				}
			}
		}
	}
}
