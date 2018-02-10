using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class Level1BossObserver : MonoBehaviour
	{
		Level1BossSequence _bossSequence;

		void Awake()
		{
			if (!Application.loadedLevelName.StartsWith("Level1_"))
			{
				Destroy(this);
				return;
			}

			var bossObj = GameObject.Find("BossSequence");
			if (bossObj == null)
			{
				Debug.Log($"{nameof(Level1BossObserver)}: Couldn't find BossSequence object");
				Destroy(this);
				return;
			}

			_bossSequence = bossObj.GetComponent<Level1BossSequence>();
		}

		void OnEnable()
		{
			_bossSequence.bossFightFinished += BossSequence_bossFightFinished;
		}

		void OnDisable()
		{
			if (_bossSequence != null)
				_bossSequence.bossFightFinished -= BossSequence_bossFightFinished;
		}

		void BossSequence_bossFightFinished()
		{
			Debug.Log("OnLevel1BossEnd: " + DebugBeatListener.DebugStr);
			SpeedrunTimer.Instance?.CompleteLevel(1);
			OldSpeedrunTimer.Instance?.CompleteLevel(1);
		}
	}
}
