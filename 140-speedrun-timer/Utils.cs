using System;

namespace SpeedrunTimerMod
{
	static class Utils
	{
		public static string FormatTime(double totalSeconds)
		{
			var seconds = totalSeconds % 60;
			var minutes = (int)(totalSeconds / 60);
			var miliseconds = Math.Truncate((totalSeconds - (int)totalSeconds) * 1000);

			var minutesStr = minutes.ToString();
			var secondsStr = ((int)seconds).ToString().PadLeft(2, '0');
			var millisecondsStr = miliseconds.ToString().PadLeft(3, '0');

			return $"{minutes}:{secondsStr}.{millisecondsStr}";
		}

		public static string FormatVersion(Version ver)
		{
			if (ver == null)
				return null;

			var Build = ver.Build > 0 ? $".{ver.Build}" : "";
			return $"{ver.Major}.{ver.Minor}{Build}";
		}
	}
}
