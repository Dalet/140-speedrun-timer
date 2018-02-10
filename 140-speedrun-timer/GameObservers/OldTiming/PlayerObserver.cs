using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod.GameObservers.OldTiming
{
	[GameObserver]
	class PlayerObserver : MonoBehaviour
	{
		static FieldInfo _playerControlPaused;
		static FieldInfo _playerMoveDirection;

		MyCharacterController _player;
		bool _prevIsDead;
		bool _isInMenu;
		bool _spawned;

		static PlayerObserver()
		{
			_playerControlPaused = typeof(MyCharacterController).GetField("controlPaused",
				BindingFlags.NonPublic | BindingFlags.Instance);
			_playerMoveDirection = typeof(MyCharacterController).GetField("moveDirection",
				BindingFlags.NonPublic | BindingFlags.Instance);
		}

		void Start()
		{
			_player = Globals.player.GetComponent<MyCharacterController>();
			_isInMenu = Application.loadedLevelName == "Level_Menu";
		}

		void LateUpdate()
		{
			CheckResumeAfterDeath();
			CheckSpawnLanding();
		}

		void CheckResumeAfterDeath()
		{
			if (!_isInMenu || OldSpeedrunTimer.Instance.IsRunning || ModLoader.Settings.ILMode)
				return;

			if (_prevIsDead && !_player.IsDead())
			{
				OldSpeedrunTimer.Instance.StartTimer();
			}
			_prevIsDead = _player.IsDead();
		}

		void CheckSpawnLanding()
		{
			if (_spawned || _isInMenu)
				return;

			if (!_player.IsLogicPause() && _player.IsOnGround()
				&& GetControlPaused() && GetMoveDirection().y == 0f)
			{
				_spawned = true;

				var level = (int)char.GetNumericValue(Application.loadedLevelName[5]);
				if (level > 0)
				{
					Debug.Log($"Level {level} started (old timing)\n" + DebugBeatListener.DebugStr);
					OldSpeedrunTimer.Instance.LevelStart(level);
				}

				OldSpeedrunTimer.Instance.EndLoad();

				if (ModLoader.Settings.ILMode)
				{
					OldSpeedrunTimer.Instance.StartTimer();
				}
			}
		}

		bool GetControlPaused()
		{
			return (bool)_playerControlPaused.GetValue(_player);
		}

		Vector3 GetMoveDirection()
		{
			return (Vector3)_playerMoveDirection.GetValue(_player);
		}
	}
}
