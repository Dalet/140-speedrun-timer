using System;
using System.Reflection;

namespace SpeedrunTimerMod.Logging
{
	public class LevelLog
	{
		public SpeedrunTime StartTime { get; set; }
		public SpeedrunTime SplitTime { get; set; }
		public SpeedrunTime Time => SplitTime - StartTime;
		public DateTime StartDate { get; set; }
		public bool IsMirrored { get; set; }
		public bool CheatsEnabled { get; set; }
	}

	public class RunLog
	{
		const int LEVEL_COUNT = 4;

		public LevelLog[] Levels { get; set; }
		public DateTime StartDate { get; set; }
		public bool IsLegacy { get; set; }
		public Version Version { get; set; }

		public RunLog()
		{
			Levels = new LevelLog[LEVEL_COUNT];
			IsLegacy = ModLoader.IsLegacyVersion;
			Version = Assembly.GetExecutingAssembly().GetName().Version;
		}

		public void LevelStart(int level, SpeedrunTime timestamp)
		{
			ThrowIfLevelOutOfRange(level);

			Levels[level - 1] = new LevelLog()
			{
				StartDate = DateTime.UtcNow,
				StartTime = timestamp,
				IsMirrored = MirrorModeManager.mirrorModeActive
			};
		}

		public void CompleteLevel(int level, SpeedrunTime timestamp)
		{
			ThrowIfLevelOutOfRange(level);

			var index = level - 1;
			if (Levels[index] == null)
				Levels[index] = new LevelLog();
			Levels[index].SplitTime = timestamp;
			Levels[index].CheatsEnabled = Cheats.Enabled;
		}

		public bool IsLevelDone(int level)
		{
			ThrowIfLevelOutOfRange(level);
			var levelLog = Levels[level - 1];
			return levelLog != null && levelLog.SplitTime.RealTime != TimeSpan.Zero;
		}

		void ThrowIfLevelOutOfRange(int level)
		{
			if (level < 1 || level > LEVEL_COUNT)
				throw new ArgumentOutOfRangeException(nameof(level));
		}
	}
}
