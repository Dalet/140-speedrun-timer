using SpeedrunTimerMod.BeatTiming;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class BeatTimerController : MonoBehaviour
	{
		public BeatTimer BeatTimer { get; set; }

		bool _firstBeatAfterReset;
		bool _firstLevelBeat;
		float _lastBeatTimestamp;
		float _lastBeatResetTimestamp;

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

		void OnLevelWasLoaded(int level)
		{
			SubscribeGlobalBeatMaster();
			_firstLevelBeat = true;
		}

		void UnsubGlobalBeatMaster()
		{
			Globals.beatMaster.onBeat -= OnGlobalBeat;
			Globals.beatMaster.onBeatReset -= OnGlobalBeatReset;
		}

		void SubscribeGlobalBeatMaster()
		{
			UnsubGlobalBeatMaster();
			Globals.beatMaster.onBeat += OnGlobalBeat;
			Globals.beatMaster.onBeatReset += OnGlobalBeatReset;
		}

		void OnGlobalBeatReset()
		{
			_firstBeatAfterReset = true;
			_lastBeatResetTimestamp = Time.time;
		}

		void OnGlobalBeat(int index)
		{
			if (!_firstLevelBeat)
			{
				if (!_firstBeatAfterReset)
					BeatTimer?.OnQuarterBeat();
				else
					_firstBeatAfterReset = false;
			}
			else
			{
				_firstLevelBeat = false;
				OnLevelFirstBeat();
			}

			_lastBeatTimestamp = Time.time;
		}

		void OnLevelFirstBeat()
		{
			if (!BeatTimer.IsStarted)
				return;

			BeatTimer.AddRealTime((int)BeatTimer.GetInterpolation());
			BeatTimer.ResetInterpolation();

			if (Application.loadedLevelName == "Level_Menu")
				BeatTimer.ResumeTimer();
		}
	}
}
