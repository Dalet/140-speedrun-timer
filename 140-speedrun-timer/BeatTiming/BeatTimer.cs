using System;
using System.Diagnostics;

namespace SpeedrunTimerMod.BeatTiming
{
	public class BeatTimer
	{
		public int Bpm { get; private set; }
		public bool IsStarted { get; private set; }
		public bool IsPaused { get; private set; }

		BeatTime _realTime;
		BeatTime _timePaused;
		BeatTime _pauseTimestamp;
		Stopwatch _interbeatStopwatch;

		public BeatTimer(int bpm)
		{
			Bpm = bpm;
			_interbeatStopwatch = new Stopwatch();
			_realTime = new BeatTime(Bpm);
			_timePaused = new BeatTime(Bpm);
		}

		public BeatTime RealTime => _realTime;

		public BeatTime Time
		{
			get
			{
				return _realTime - TimePaused;
			}
		}

		public TimeSpan InterpolatedRealTime
		{
			get
			{
				if (!IsStarted)
					return TimeSpan.Zero;
				return RealTime.TimeSpan + _interbeatStopwatch.Elapsed;
			}
		}

		public TimeSpan InterpolatedTime
		{
			get
			{
				if (!IsStarted || IsPaused)
					return Time.TimeSpan;

				return Time.TimeSpan + _interbeatStopwatch.Elapsed;
			}
		}

		public BeatTime TimePaused
		{
			get
			{
				if (!IsPaused)
					return _timePaused;
				return _timePaused + _realTime - _pauseTimestamp;
			}
		}

		public void PauseInterpolation() => _interbeatStopwatch.Stop();
		public void ResumeInterpolation() => _interbeatStopwatch.Start();
		public long GetInterpolation() => _interbeatStopwatch.ElapsedMilliseconds;

		public void ResetInterpolation()
		{
			_interbeatStopwatch.Reset();
			_interbeatStopwatch.Start();
		}

		public void OnQuarterBeat()
		{
			ResetInterpolation();

			if (IsStarted)
			{
				_realTime = _realTime.AddQuarterBeats(1);
			}
		}

		public void StartTimer(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			if (IsStarted)
				return;

			_realTime = new BeatTime(Bpm, quarterBeatsOffset * -1, millisecondsOffset * -1);
			_interbeatStopwatch.Start();
			IsStarted = true;
		}

		public void ResumeTimer(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			if (!IsPaused)
				return;

			var unpauseTimestamp = _realTime + new BeatTime(Bpm, quarterBeatsOffset, millisecondsOffset);
			_timePaused += unpauseTimestamp - _pauseTimestamp;
			IsPaused = false;
		}

		public void PauseTimer(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			if (!IsStarted || IsPaused)
				return;

			IsPaused = true;
			_pauseTimestamp = _realTime + new BeatTime(Bpm, quarterBeatsOffset, millisecondsOffset);
		}

		public void ResetTimer()
		{
			IsStarted = IsPaused = false;
			_realTime = _timePaused = _pauseTimestamp = new BeatTime(Bpm);
			_interbeatStopwatch.Stop();
			_interbeatStopwatch.Reset();
		}

		public void AddRealTime(int milliseconds)
		{
			_realTime = _realTime.AddOffset(milliseconds);

			// time paused needs to be adjusted so game time stays the same
			if (!IsPaused)
				_timePaused = _timePaused.AddOffset(milliseconds);
		}
	}
}
