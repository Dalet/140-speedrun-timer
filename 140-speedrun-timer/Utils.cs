using System;
using System.Diagnostics;
using UnityEngine;

namespace SpeedrunTimerMod
{
	static class Utils
	{
		public static string FormatTime(double totalSeconds, int decimals = 2)
		{
			totalSeconds = Math.Round(totalSeconds, decimals, MidpointRounding.AwayFromZero);

			var seconds = totalSeconds % 60;
			var minutes = (int)(totalSeconds / 60);
			var milliseconds = Math.Round((totalSeconds - (int)totalSeconds) * Math.Pow(10, decimals));

			var minutesStr = minutes.ToString();
			var secondsStr = ((int)seconds).ToString().PadLeft(2, '0');
			var millisecondsStr = milliseconds.ToString().PadLeft(decimals, '0');

			return $"{minutes}:{secondsStr}.{millisecondsStr}";
		}

		public static string FormatVersion(Version ver)
		{
			if (ver == null)
				return null;

			var Build = ver.Build > 0 ? $".{ver.Build}" : "";
			return $"{ver.Major}.{ver.Minor}{Build}";
		}

		public static double ElapsedSeconds(this Stopwatch sw)
		{
			return sw.ElapsedTicks / (double)TimeSpan.TicksPerSecond;
		}

		internal class Label
		{
			public bool enabled = true;
			public Rect position;
			public GUIStyle style;
			public string text;

			public void OnGUI(string newText = null)
			{
				if (newText != null)
					text = newText;

				if (enabled)
					GUI.Label(position, text, style);
			}
		}
	}
}
