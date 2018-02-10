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
			var beatStartTime = !_endSoundPlaying ? -1000 : -3000;
			Debug.Log($"GlobalBeatStarted: added {beatStartTime}ms to timer\n"
				+ DebugBeatListener.DebugStr);

			/* TIMELINE
			 *
			 * Normal Mode
			 * RealT, GameT
			 * 00:59, 00:59 - Level load ends, Game Time is still paused
			 * 01:00, 00:59 - Beat started 00:01 ago
			 * 01:00, 00:59 => 01:00, 01:00 - [END LOAD, -00:01 offset]
			 * 01:00, 01:00 - [LEVEL START, -00:01 offset] Level starts at 00:59, 00:59
			 *
			 * IL mode
			 * RealT, GameT
			 * 00:00, 00:00 - Level load ends, timer is not running
			 * 00:00, 00:00 - Beat started 00:01 ago
			 * 00:00, 00:00 => 00:01, 00:01 - [START TIMER, -00:01 offset]
			 * 00:01, 00:01 - [LEVEL START, -00:01 offset] Level starts at 00:00, 00:00
			 */

			SpeedrunTimer.Instance.EndLoad(beatStartTime);

			if (!_isInMenu)
			{
				if (ModLoader.Settings.ILMode)
				{
					SpeedrunTimer.Instance.StartTimer(beatStartTime);
				}

				var level = (int)char.GetNumericValue(Application.loadedLevelName[5]);
				if (level > 0)
				{
					Debug.Log($"Level {level} started\n" + DebugBeatListener.DebugStr);
					SpeedrunTimer.Instance.LevelStart(level, beatStartTime);
				}
			}
		}
	}
}
