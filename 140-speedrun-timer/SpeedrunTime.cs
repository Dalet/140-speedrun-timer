using SpeedrunTimerMod.BeatTiming;
using System;

namespace SpeedrunTimerMod
{
	public struct SpeedrunTime
	{
		public TimeSpan RealTime { get; }
		public TimeSpan GameTime { get; }
		public BeatTime GameBeatTime { get; }

		public bool HasBeatTime => GameTime == TimeSpan.Zero || GameBeatTime.Milliseconds != 0;

		public SpeedrunTime(TimeSpan realTime, TimeSpan gameTime)
		{
			RealTime = realTime;
			GameTime = gameTime;
			GameBeatTime = new BeatTime();
		}

		public SpeedrunTime(TimeSpan realTime, BeatTime gameTime)
			: this(realTime, gameTime.TimeSpan)
		{
			GameBeatTime = gameTime;
		}

		public static SpeedrunTime operator +(SpeedrunTime left, SpeedrunTime right)
		{
			var realTime = left.RealTime + right.RealTime;

			if (left.HasBeatTime && right.HasBeatTime)
			{
				var gameTime = left.GameBeatTime + right.GameBeatTime;
				return new SpeedrunTime(realTime, gameTime);
			}

			var gameTimeTs = left.GameTime + right.GameTime;
			return new SpeedrunTime(realTime, gameTimeTs);
		}

		public static SpeedrunTime operator -(SpeedrunTime left, SpeedrunTime right)
		{
			var realTime = left.RealTime - right.RealTime;

			if (left.HasBeatTime && right.HasBeatTime)
			{
				var gameTime = left.GameBeatTime - right.GameBeatTime;
				return new SpeedrunTime(realTime, gameTime);
			}

			var gameTimeTs = left.GameTime - right.GameTime;
			return new SpeedrunTime(realTime, gameTimeTs);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SpeedrunTime))
				return false;
			return (SpeedrunTime)obj == this;
		}

		public static bool operator ==(SpeedrunTime left, SpeedrunTime right)
		{
			return left.RealTime == right.RealTime && left.GameTime == right.GameTime;
		}

		public static bool operator !=(SpeedrunTime left, SpeedrunTime right)
		{
			return !(left == right);
		}

		public override string ToString()
		{
			return $"{RealTime}, {GameTime}";
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = 0;
				result = (result * 397) ^ (int)RealTime.Ticks;
				result = (result * 397) ^ (int)GameTime.Ticks;
				result = (result * 397) ^ (int)GameBeatTime.Milliseconds;
				return result;
			}
		}
	}
}
