using System;
using System.Diagnostics;

namespace SpeedrunTimerMod.BeatTiming
{
	public class BeatTimer
	{
		public int Bpm { get; private set; }
		public bool IsPaused { get; private set; }
		public bool IsStarted { get; private set; }
		public BeatTime Time { get; private set; }

		Stopwatch _interbeatStopwatch;

		public BeatTimer(int bpm)
		{
			Bpm = bpm;
			_interbeatStopwatch = new Stopwatch();
			Time = new BeatTime(Bpm);
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

			if (IsStarted && !IsPaused)
			{
				Time = Time.AddQuarterBeats(1);
			}
		}

		public void StartTimer(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			if (IsStarted)
				return;

			Time = new BeatTime(Bpm, quarterBeatsOffset * -1, millisecondsOffset * -1);
			_interbeatStopwatch.Start();
			IsStarted = true;
		}

		public void ResumeTimer(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			if (!IsPaused)
				return;

			var offset = new BeatTime(Bpm, quarterBeatsOffset, millisecondsOffset);
			Time -= offset;
			IsPaused = false;
		}

		public void PauseTimer(int millisecondsOffset = 0, int quarterBeatsOffset = 0)
		{
			if (!IsStarted || IsPaused)
				return;

			var offset = new BeatTime(Bpm, quarterBeatsOffset, millisecondsOffset);
			Time += offset;
			IsPaused = true;
		}

		public void ResetTimer()
		{
			IsStarted = IsPaused = false;
			Time = new BeatTime(Bpm);
			_interbeatStopwatch.Stop();
			_interbeatStopwatch.Reset();
		}

		public void AddTime(int milliseconds)
		{
			if (!IsStarted)
				return;

			Time = Time.AddOffset(milliseconds);
		}
	}
}
