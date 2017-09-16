using System;

namespace SpeedrunTimerMod.BeatTiming
{
	public struct BeatTime
	{
		public readonly int bpm;
		public readonly int quarterBeatCount;

		/// <summary>
		/// Offset in milliseconds
		/// </summary>
		public readonly int offset;

		public BeatTime(int bpm, int quarterBeatCount = 0, int offset = 0)
		{
			this.bpm = bpm;
			this.quarterBeatCount = quarterBeatCount;
			this.offset = offset;
		}

		public long Milliseconds
		{
			get
			{
				var quarterBeatinterval = GetQuarterBeatInterval(bpm);
				return (long)Math.Round(quarterBeatinterval * quarterBeatCount) + offset;
			}
		}

		public TimeSpan TimeSpan => TimeSpan.FromMilliseconds(Milliseconds);

		public BeatTime AddQuarterBeats(int quarterBeats)
		{
			return new BeatTime(bpm, quarterBeatCount + quarterBeats, offset);
		}

		public BeatTime AddOffset(int milliseconds)
		{
			return new BeatTime(bpm, quarterBeatCount, offset + milliseconds);
		}

		public static BeatTime operator +(BeatTime left, BeatTime right)
		{
			if (left.bpm != right.bpm)
				throw new ArgumentException("The BPMs must match.");

			var quarterBeats = left.quarterBeatCount + right.quarterBeatCount;
			var offset = left.offset + right.offset;
			return new BeatTime(left.bpm, quarterBeats, offset);
		}

		public static BeatTime operator -(BeatTime left, BeatTime right)
		{
			if (left.bpm != right.bpm)
				throw new ArgumentException("The BPMs must match.");

			var quarterBeats = left.quarterBeatCount - right.quarterBeatCount;
			var offset = left.offset - right.offset;
			return new BeatTime(left.bpm, quarterBeats, offset);
		}

		public static decimal GetQuarterBeatInterval(int bpm)
		{
			return (decimal)60 / bpm / 4 * 1000;
		}

		public override string ToString()
		{
			var sign = offset < 0 ? '-' : '+';
			return $"{quarterBeatCount}qb@{bpm}{sign}{offset}ms";
		}
	}
}
