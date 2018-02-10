using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class Level3BossObserver : MonoBehaviour
	{
		BossSphereArena _bossArena;

		void Awake()
		{
			if (!Application.loadedLevelName.StartsWith("Level3_"))
			{
				Destroy(this);
				return;
			}

			var bossObj = GameObject.Find("BossArenaTransform");
			if (bossObj == null)
			{
				Debug.Log($"{nameof(Level3BossObserver)}: Couldn't find BossArenaTransform object");
				Destroy(this);
				return;
			}

			_bossArena = bossObj.GetComponent<BossSphereArena>();
		}

		void OnEnable()
		{
			if (_bossArena != null)
				_bossArena.pattern.part3DoneEvent += Pattern_part3DoneEvent;
		}

		void OnDisable()
		{
			if (_bossArena != null)
				_bossArena.pattern.part3DoneEvent -= Pattern_part3DoneEvent;
		}

		void Pattern_part3DoneEvent()
		{
			Debug.Log("OnLevel3BossEnd: " + DebugBeatListener.DebugStr);
			SpeedrunTimer.Instance?.CompleteLevel(3);
			SpeedrunTimer.Instance?.StartLoad(39000, 64);
			OldSpeedrunTimer.Instance?.StartLoad();
			OldSpeedrunTimer.Instance?.CompleteLevel(3);
		}
	}
}
