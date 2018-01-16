using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class LevelObserver : MonoBehaviour
	{
		bool _isInMenu;
		bool _endSoundPlaying;

		void Start()
		{
			SubscribeGlobalBeatMaster();
			_isInMenu = Application.loadedLevelName == "Level_Menu";
			_endSoundPlaying = TheEndSound.EndSoundPlaying() && _isInMenu;

			if (!_isInMenu && !Cheats.LevelLoadedByCheat)
				SpeedrunTimer.Instance.Split();
		}

		void OnEnable()
		{
			SubscribeGlobalBeatMaster();
		}

		void OnDisable()
		{
			UnsubGlobalBeatMaster();
		}

		void SubscribeGlobalBeatMaster()
		{
			if (Globals.beatMaster == null)
				return;

			UnsubGlobalBeatMaster();
			Globals.beatMaster.globalBeatStarted += OnGlobalBeatStarted;
		}

		void UnsubGlobalBeatMaster()
		{
			if (Globals.beatMaster == null)
				return;

			Globals.beatMaster.globalBeatStarted -= OnGlobalBeatStarted;
		}

		void OnGlobalBeatStarted()
		{
			// we know the beat starts 1 second after level load
			// except when end sound is playing
			// see GlobalBeatMaster.startTime
			var beatStartTime = !_endSoundPlaying ? 1000 : 3000;

			if (SpeedrunTimer.Instance.IsRunning)
			{
				SpeedrunTimer.Instance.EndLoad(beatStartTime * -1);
			}
			else if (ModLoader.Settings.ILMode && !_isInMenu)
			{
				SpeedrunTimer.Instance.StartTimer(beatStartTime * -1);
			}

			Debug.Log($"GlobalBeatStarted: added {beatStartTime}ms to timer\n"
				+ DebugBeatListener.DebugStr);
		}
	}
}
