using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class Level4BossObserver : MonoBehaviour
	{
		GameObject _endingObj;

		void Awake()
		{
			if (ModLoader.IsLegacyVersion || !Application.loadedLevelName.StartsWith("Level4_"))
				Destroy(this);
		}

		void Start()
		{
			_endingObj = GameObject.Find("BossEnding");
			if (_endingObj == null)
			{
				Debug.Log($"{nameof(Level4BossObserver)}: Couldn't find BossEnding object");
				Destroy(this);
			}
		}

		void LateUpdate()
		{
			if (_endingObj != null && _endingObj.activeSelf)
			{
				_endingObj = null;
				Debug.Log("OnLevel4BossEnd: " + DebugBeatListener.DebugStr);
				SpeedrunTimer.Instance?.CompleteLevel(4);
				OldSpeedrunTimer.Instance?.CompleteLevel(4);
			}
		}
	}
}
