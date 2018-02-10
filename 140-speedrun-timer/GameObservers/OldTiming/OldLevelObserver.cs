using UnityEngine;

namespace SpeedrunTimerMod.GameObservers.OldTiming
{
	[GameObserver]
	class OldLevelObserver : MonoBehaviour
	{
		TunnelBossEndSequence _level2Tunnel;

		void Start()
		{
			var menuSystemObj = GameObject.Find("_MenuSystem");
			if (menuSystemObj != null)
			{
				var menuSystem = menuSystemObj.GetComponent<MenuSystem>();
				menuSystem.colorSphere.colorSphereOpened += OnMenuKeyUsed;
			}

			if (Application.loadedLevelName.StartsWith("Level2_"))
			{
				var bossObj = GameObject.Find("BossPart3");
				if (bossObj != null)
				{
					_level2Tunnel = bossObj.GetComponent<TunnelBossEndSequence>();
				}
			}
		}

		void Update()
		{
			if (_level2Tunnel != null)
			{
				var leftWall = _level2Tunnel.endWallLeft.GetComponent<Renderer>().bounds;
				var rightWall = _level2Tunnel.endWallRight.GetComponent<Renderer>().bounds;
				if (leftWall.Intersects(rightWall))
				{
					_level2Tunnel = null;
					OnLevel2BossEnd();
				}
			}
		}

		void OnLevel2BossEnd()
		{
			Debug.Log("OldTiming Level 2 end");
			OldSpeedrunTimer.Instance?.CompleteLevel(2);
		}

		void OnMenuKeyUsed()
		{
			OldSpeedrunTimer.Instance?.StartLoad();
			OldSpeedrunTimer.Instance?.Unfreeze();
		}
	}
}
