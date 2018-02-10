using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class Level2BossObserver : MonoBehaviour
	{
		TunnelBossEndSequence _tunnelSequence;
		bool _checkScrollEnded;

		void Awake()
		{
			if (!Application.loadedLevelName.StartsWith("Level2_"))
			{
				Destroy(this);
				return;
			}

			var bossObj = GameObject.Find("BossPart3");
			if (bossObj == null)
			{
				Debug.Log($"{nameof(Level2BossObserver)}: Couldn't find BossPart3 object");
				Destroy(this);
				return;
			}

			_tunnelSequence = bossObj.GetComponent<TunnelBossEndSequence>();
		}

		void OnEnable()
		{
			if (_tunnelSequence != null)
				_tunnelSequence.endScrollingBeat.onBeat += EndScrollingBeat_onBeat;
		}

		void OnDisable()
		{
			if (_tunnelSequence != null)
				_tunnelSequence.endScrollingBeat.onBeat -= EndScrollingBeat_onBeat;
		}

		void EndScrollingBeat_onBeat()
		{
			_checkScrollEnded = true;
		}

		void LateUpdate()
		{
			if (!_checkScrollEnded)
				return;

			_checkScrollEnded = false;

			if (_tunnelSequence.endWallLeft.GetComponent<Renderer>().enabled)
				OnLevel2BossEnd();
		}

		void OnLevel2BossEnd()
		{
			Debug.Log("OnLevel2BossEnd: " + DebugBeatListener.DebugStr);
			SpeedrunTimer.Instance?.CompleteLevel(2);
			this.enabled = false;
		}
	}
}
