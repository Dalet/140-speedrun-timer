using SpeedrunTimerMod.BeatTiming;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class BeatTimerController : MonoBehaviour
	{
		public BeatTimer BeatTimer { get; set; }

		bool _firstLevelUpdate;
		bool _endSoundPlaying;
		bool _firstBeatAfterReset;

		void OnLevelWasLoaded(int level)
		{
			SubscribeGlobalBeatMaster();
			_firstLevelUpdate = true;
		}

		void Start()
		{
			SubscribeGlobalBeatMaster();
			_endSoundPlaying = TheEndSound.EndSoundPlaying() && Application.loadedLevelName == "Level_Menu";
		}

		void OnEnable()
		{
			if (Globals.beatMaster != null)
				SubscribeGlobalBeatMaster();
		}

		void OnDisable()
		{
			UnsubGlobalBeatMaster();
		}

		void UnsubGlobalBeatMaster()
		{
			Globals.beatMaster.onBeat -= OnGlobalBeat;
			Globals.beatMaster.onBeatReset -= OnGlobalBeatReset;
			Globals.beatMaster.globalBeatStarted -= OnGlobalBeatStarted;
		}

		void SubscribeGlobalBeatMaster()
		{
			UnsubGlobalBeatMaster();
			Globals.beatMaster.onBeat += OnGlobalBeat;
			Globals.beatMaster.onBeatReset += OnGlobalBeatReset;
			Globals.beatMaster.globalBeatStarted += OnGlobalBeatStarted;
		}

		void Update()
		{
			if (_firstLevelUpdate)
			{
				// this is when time starts counting down from the beat start offset
				_firstLevelUpdate = false;
				if (BeatTimer != null && BeatTimer.IsStarted)
				{
					BeatTimer.AddTime((int)BeatTimer.GetInterpolation());
					BeatTimer.ResetInterpolation();
				}
			}
		}

		void OnGlobalBeatStarted()
		{
			Debug.Log("GlobalBeatStarted: " + DebugBeatListener.DebugStr);
			if (BeatTimer == null || !BeatTimer.IsStarted)
				return;

			// we know the beat starts 1 second after level load
			// except when end sound is playing
			// see GlobalBeatMaster.startTime
			var beatStartTime = !_endSoundPlaying ? 1000 : 3000;

			BeatTimer.AddTime(beatStartTime);
			BeatTimer.ResetInterpolation();
			SpeedrunTimer.Instance.EndLoad(beatStartTime * -1);
		}

		void OnGlobalBeatReset()
		{
			_firstBeatAfterReset = true;
		}

		void OnGlobalBeat(int index)
		{
			if (!_firstBeatAfterReset)
				BeatTimer?.OnQuarterBeat();
			else
				_firstBeatAfterReset = false;
		}
	}
}
