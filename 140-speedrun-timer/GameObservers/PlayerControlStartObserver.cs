using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class PlayerControlStartObserver : MonoBehaviour
	{
		MyCharacterController _player;
		PlayerControlOverride _playerOverride;
		bool _firstResumeControl = true;

		void Awake()
		{
			if (Application.loadedLevelName == "Level_Menu")
				Destroy(this);
		}

		void Start()
		{
			_player = Globals.player.GetComponent<MyCharacterController>();
			_playerOverride = ModLoader.LevelObject.GetComponent<PlayerControlOverride>();
		}

		void OnEnable()
		{
			Hooks.OnPlayerResumeControl += OnPlayerResumeControl;
		}

		void OnDisable()
		{
			Hooks.OnPlayerResumeControl -= OnPlayerResumeControl;
		}

		void OnPlayerResumeControl()
		{
			if (!_firstResumeControl || _player == null || _playerOverride == null)
				return;

			_firstResumeControl = false;

			if (!_player.IsOnGround())
				return;

			int[] beats = null;
			switch (Application.loadedLevelName.Substring(0, 6))
			{
				case "Level1":
					beats = new int[] { 2 };
					break;
				case "Level2":
					beats = new int[] { 1 };
					break;
				case "Level3":
					beats = Utils.GetSimilarQuarterBeats(11);
					break;
				case "Level4":
					beats = new int[] { 3 };
					break;
			}

			_playerOverride.SetCallback(() => SpeedrunTimer.Instance?.EndLoad());
			_playerOverride.HoldUntilBeat(beats);
		}
	}
}
