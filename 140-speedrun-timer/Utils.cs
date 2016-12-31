using System;
using System.Diagnostics;
using UnityEngine;

namespace SpeedrunTimerMod
{
	internal static class Utils
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

		public static void PlayerPrefsSetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
		}

		public static bool PlayerPrefsGetBool(string key, bool def = default(bool))
		{
			if (PlayerPrefs.HasKey(key))
				return PlayerPrefs.GetInt(key, def ? 1 : 0) != 0;
			return def;
		}

		// https://stackoverflow.com/questions/2353211/hsl-to-rgb-color-conversion
		public static Color HslToRgba(float h, float s, float l, float a)
		{
			float r, g, b;

			if (s == 0.0f)
				r = g = b = l;
			else
			{
				var q = l < 0.5f ? l * (1.0f + s) : l + s - l * s;
				var p = 2.0f * l - q;
				r = HueToRgb(p, q, h + 1.0f / 3.0f);
				g = HueToRgb(p, q, h);
				b = HueToRgb(p, q, h - 1.0f / 3.0f);
			}

			return new Color(r, g, b, a);
		}

		static float HueToRgb(float p, float q, float t)
		{
			if (t < 0.0f) t += 1.0f;
			if (t > 1.0f) t -= 1.0f;
			if (t < 1.0f / 6.0f) return p + (q - p) * 6.0f * t;
			if (t < 1.0f / 2.0f) return q;
			if (t < 2.0f / 3.0f) return p + (q - p) * (2.0f / 3.0f - t) * 6.0f;
			return p;
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
