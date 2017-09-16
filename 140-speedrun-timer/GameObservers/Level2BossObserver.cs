using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class Level2BossObserver : MonoBehaviour
	{
		MyCharacterController _player;
		TunnelBossEndSequence _tunnelSequence;

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

		void Start()
		{
			_player = Globals.player.GetComponent<MyCharacterController>();
		}

		void OnEnable()
		{
			_tunnelSequence.evilBlock.attack += EvilBlock_attack;
		}

		void OnDisable()
		{
			if (_tunnelSequence != null)
				_tunnelSequence.evilBlock.attack -= EvilBlock_attack;
			if (_player != null)
				_player.Killed -= Player_Killed;
		}

		void EvilBlock_attack(int attackIndex)
		{
			if (attackIndex != 10)
				return;

			_player.Killed += Player_Killed;
			OnLevel2BossEnd();
		}

		void Player_Killed()
		{
			_player.Killed -= Player_Killed;
			SpeedrunTimer.Instance?.Unsplit();
		}

		void OnLevel2BossEnd()
		{
			Debug.Log("OnLevel2BossEnd: " + DebugBeatListener.DebugStr);
			SpeedrunTimer.Instance?.CompleteLevel(2);
		}
	}
}
