using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace SpeedrunTimerMod.Logging
{
	public class RunLogCsvWriter
	{
		public RunLog Log { get; set; }
		public RunLog OldTimingLog { get; set; }

		public bool Level1 { get; set; }
		public bool Level2 { get; set; }
		public bool Level3 { get; set; }
		public bool Level4 { get; set; }

		public bool AnyPercent { get; set; }
		public bool AllLevels { get; set; }

		public RunLogCsvWriter(RunLog log, RunLog oldTimingLog)
		{
			if (log == null || oldTimingLog == null)
				throw new ArgumentNullException();

			Log = log;
			OldTimingLog = oldTimingLog;
		}

		public IEnumerator WriteToLogAsyncOnFrameEnd()
		{
			yield return new WaitForEndOfFrame();
			WriteToLogAsync();
		}

		public void WriteToLog()
		{
			if (!AnyPercent && !AllLevels && CountSelectedLevels() == 0)
				throw new InvalidOperationException("No level selected.");

			RunLogFile.WriteLine(GetCsv());
		}

		public void WriteToLogAsync()
		{
			if (!AnyPercent && !AllLevels && CountSelectedLevels() == 0)
				throw new InvalidOperationException("No level selected.");

			RunLogFile.WriteLineAsync(GetCsv());
		}

		public string GetCsv()
		{
			var str = string.Empty;

			for (var i = 0; i < 4; i++)
			{
				if (CheckIfPrintLevel(i))
					str += GetIndividualLevelCsv(i);
			}

			if (AnyPercent)
			{
				str += GetAnyPercentCsv();
			}

			if (AllLevels)
			{
				str += GetAllLevelsCsv();
			}

			return str;
		}

		string GetAnyPercentCsv() => GetFullGameCsv("ANY%", 2);

		string GetAllLevelsCsv() => GetFullGameCsv("ALL LEVELS", 3);

		string GetFullGameCsv(string categoryName, int lastSplitIndex)
        {
			var lastLevel = Log.Levels[lastSplitIndex];
			var lastLevelOld = OldTimingLog.Levels[lastSplitIndex];

			return GetLogCsv(categoryName, Log.StartDate, lastLevel.SplitTime,
				lastLevelOld.SplitTime,	lastLevel.IsMirrored, lastLevel.CheatsEnabled);
		}

		string GetIndividualLevelCsv(int levelIndex)
		{
			var levelLog = Log.Levels[levelIndex];
			var levelLogOldTiming = OldTimingLog.Levels[levelIndex];

			return GetLogCsv($"LEVEL {levelIndex + 1}", levelLog.StartDate, levelLog.Time,
				levelLogOldTiming.Time, levelLog.IsMirrored, levelLog.CheatsEnabled);
		}

		string GetLogCsv(string category, DateTime startDate, SpeedrunTime time,
			SpeedrunTime oldTimingTime, bool isMirrored,  bool cheatsEnabled)
		{
			if (startDate.Kind == DateTimeKind.Utc)
				startDate = startDate.ToLocalTime();
			var startDateStr = startDate.ToString("o", CultureInfo.InvariantCulture);

			var decimals = 3;
			var realTimeStr = Utils.FormatTime(time.RealTime, decimals);
			var gameTimeStr = Utils.FormatTime(time.GameTime, decimals);

			var realTimeOldStr = Utils.FormatTime(oldTimingTime.RealTime, decimals);
			var gameTimeOldStr = Utils.FormatTime(oldTimingTime.GameTime, decimals);

			var version = Log.Version;
			var gameVersion = Log.IsLegacy ? "2013" : "2017";
			var cheats = BoolToYesNo(cheatsEnabled);
			var mirrored = BoolToYesNo(isMirrored);

			return category + ",START DATE,REAL TIME,GAME TIME,GAME TIME (RAW)"
				+ ",REAL TIME (OLD TIMING),GAME TIME (OLD TIMING)"
				+ ",MIRRORED,CHEATS,MOD VERSION,GAME VERSION"
				+ Environment.NewLine
				+ $",{startDateStr},{realTimeStr},{gameTimeStr},{time.GameBeatTime}"
				+ $",{realTimeOldStr},{gameTimeOldStr}"
				+ $",{mirrored},{cheats},{Log.Version},{gameVersion}"
				+ Environment.NewLine;
		}

		string BoolToYesNo(bool b) => b ? "Yes" : "No";

		int CountSelectedLevels()
		{
			var count = 0;
			for (var i = 0; i < 4; i++)
			{
				if (CheckIfPrintLevel(i))
					count++;
			}
			return count;
		}

		bool CheckIfPrintLevel(int levelIndex)
		{
			switch (levelIndex)
			{
				case 0:
					return Level1;
				case 1:
					return Level2;
				case 2:
					return Level3;
				case 3:
					return Level4;
				default:
					return false;
			}
		}
	}
}
