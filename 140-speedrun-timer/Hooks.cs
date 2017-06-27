using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class Hooks : MonoBehaviour
	{
		static FieldInfo isInChargeStateField;
		TunnelBossEndSequence _level2Tunnel;

		void OnLevelWasLoaded(int level)
		{
			if (!SpeedrunTimerLoader.IsLegacyVersion)
			{
				if (isInChargeStateField == null)
					isInChargeStateField = typeof(ColorSphere).GetField("public_isInChargeState");
				isInChargeStateField.SetValue(null, false);
			}
		}

		void Start()
		{
			var menuSystemObj = GameObject.Find("_MenuSystem");
			if (menuSystemObj != null)
			{
				var menuSystem = menuSystemObj.GetComponent<MenuSystem>();
				menuSystem.colorSphere.colorSphereOpened += OnMenuKeyUsed;
			}
			else
				Debug.Log("Couldn't find _MenuSystem object");

			var levelName = Application.loadedLevelName;
			if (levelName.StartsWith("Level1_"))
			{
				var bossObj = GameObject.Find("BossSequence");
				if (bossObj != null)
				{
					var bossSequence = bossObj.GetComponent<Level1BossSequence>();
					bossSequence.bossFightFinished += OnLevel1BossEnd;
				}
				else
					Debug.Log("Couldn't find BossSequence object");
			}
			else if (levelName.StartsWith("Level2_"))
			{
				var bossObj = GameObject.Find("BossPart3");
				if (bossObj != null)
				{
					_level2Tunnel = bossObj.GetComponent<TunnelBossEndSequence>();
				}
				else
					Debug.Log("Couldn't find BossPart3 object");
			}
			else if (levelName.StartsWith("Level3_"))
			{
				var bossObj = GameObject.Find("BossArenaTransform");
				if (bossObj != null)
				{
					var component = bossObj.GetComponent<BossSphereArena>();
					component.pattern.part3DoneEvent += OnLevel3BossEnd;	
				}
				else
					Debug.Log("Couldn't find BossArenaTransform object");
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

		public static void OnPlayerFixedUpdate(bool logicPaused, bool controlPaused, Vector3 moveDirection)
		{
			if (!logicPaused && controlPaused && moveDirection.y == 0f)
			{
				SpeedrunTimer.Instance?.EndLoad();
			}
		}

		public static void OnLevel1BossEnd()
		{
			Debug.Log("OnLevel1BossEnd");
			SpeedrunTimer.Instance.CompleteLevel(1);
		}

		public static void OnLevel2BossEnd()
		{
			Debug.Log("OnLevel2BossEnd");
			SpeedrunTimer.Instance.CompleteLevel(2);
		}

		public static void OnLevel3BossEnd()
		{
			Debug.Log("OnLevel3BossEnd");
			SpeedrunTimer.Instance.CompleteLevel(3);
		}

		public static void OnLevel4BossEnd()
		{
			Debug.Log("OnLevel4BossEnd");
			SpeedrunTimer.Instance.CompleteLevel(4);
		}

		public static void OnMenuKeyUsed() // triggered slightly before OnKeyUsed
		{
			Debug.Log("OnMenuKeyUsed");

			SpeedrunTimer.Instance.StartLoad();
			SpeedrunTimer.Instance.Unfreeze();
		}

		public static void OnKeyUsed()
		{
			Debug.Log("OnKeyUsed");
			SpeedrunTimer.Instance.Split();
		}

		public static void OnResumeAfterDeath()
		{
			if (Application.loadedLevelName == "Level_Menu")
				SpeedrunTimer.Instance.StartTimer();
		}
	}
}
