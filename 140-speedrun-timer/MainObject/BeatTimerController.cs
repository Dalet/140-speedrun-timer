using SpeedrunTimerMod.BeatTiming;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class BeatTimerController : MonoBehaviour
	{
		public BeatTimer BeatTimer { get; set; }

		bool _firstBeatAfterReset;

		void OnLevelWasLoaded(int level)
		{
			SubscribeGlobalBeatMaster();
		}

		void Start()
		{
			SubscribeGlobalBeatMaster();
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

		void OnApplicationPause(bool pause)
		{
			if (pause)
				BeatTimer.PauseInterpolation();
			else
				BeatTimer.ResumeInterpolation();
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

		void OnGlobalBeatStarted()
		{
			if (BeatTimer == null || !BeatTimer.IsStarted)
				return;

			BeatTimer.ResetInterpolation();
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
