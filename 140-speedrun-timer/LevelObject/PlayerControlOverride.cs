using System;
using System.Linq;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class PlayerControlOverride : MonoBehaviour
	{
		event Action Released;

		MyCharacterController _player;
		bool _controlPaused;
		int[] _beatIds;

		int _currentBeatId;
		float _lastBeatTime;

		void Start()
		{
			_player = Globals.player.GetComponent<MyCharacterController>();
			Globals.beatMaster.onBeat += BeatMaster_onBeat;
			this.enabled = false;
		}

		void OnEnable()
		{
			Hooks.OnPlayerResumeControl += OnPlayerResumeControl;
		}

		void OnDisable()
		{
			Hooks.OnPlayerResumeControl -= OnPlayerResumeControl;
			_controlPaused = false;
			_beatIds = null;
			Released = null;
		}

		void OnDestroy()
		{
			Globals.beatMaster.onBeat -= BeatMaster_onBeat;
		}

		public void HoldUntilBeat(params int[] beatIds)
		{
			/*
			var timeSinceBeat = Time.time - _lastBeatTime;
			if (timeSinceBeat <= 0.02 && beatIds.Contains(_currentBeatId))
			{
				Debug.Log("PlayerControlOverride: Released control immediately");
				Released?.Invoke();
				return;
			}
			*/

			this.enabled = true;

			_beatIds = beatIds;
			_controlPaused = true;
			_player.PauseControl();
		}

		public void SetCallback(Action callback)
		{
			if (callback == null)
				throw new ArgumentNullException();

			Action eventHandler = null;
			eventHandler = () =>
			{
				Released -= eventHandler;
				callback();
			};
			Released += eventHandler;
		}

		void ReleaseControl()
		{
			_controlPaused = false;
			_player.ResumeControl();
			Released?.Invoke();
			this.enabled = false;
		}

		void BeatMaster_onBeat(int index)
		{
			_currentBeatId = index;
			_lastBeatTime = Time.time;

			if (_beatIds == null || !this.enabled)
				return;

			if (_controlPaused && (_beatIds.Length == 0 || _beatIds.Contains(index)))
			{
				Debug.Log($"PlayerControlOverride: released player control on beat #{index}");
				ReleaseControl();
			}
		}

		void OnPlayerResumeControl()
		{
			Debug.Log((_controlPaused ? "Cancelled " : "") + "Attempt to resume player control: "
				+ DebugBeatListener.DebugStr);

			if (_controlPaused)
			{
				_player.PauseControl();
			}
		}
	}
}
