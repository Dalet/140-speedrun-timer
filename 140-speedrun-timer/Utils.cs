using System;
using UnityEngine;

namespace SpeedrunTimerMod
{
	public static class Utils
	{
		public static string FormatTime(TimeSpan timespan, int decimals = 2)
		{
			timespan = TimeSpan.FromSeconds(timespan.TotalSeconds); // rounds to nearest millisecond

			var sign = string.Empty;
			if (timespan < TimeSpan.Zero)
			{
				sign = "-";
				timespan = timespan.Negate();
			}

			var hours = ((int)timespan.TotalHours);
			var hoursStr = hours > 0
				? hours.ToString() + ":"
				: string.Empty;

			var minutes = timespan.Minutes;
			var minutesStr = hours == 0 && minutes < 10
				? minutes.ToString()
				: minutes.ToString().PadLeft(2, '0');

			var secondsStr = timespan.Seconds.ToString().PadLeft(2, '0');

			var millisecondsStr = string.Empty;
			if (decimals > 0)
			{
				var totalSeconds = timespan.TotalSeconds;
				var milliseconds = Math.Round((totalSeconds - (int)totalSeconds) * Math.Pow(10, decimals));
				millisecondsStr = "." + milliseconds.ToString().PadLeft(decimals, '0');
			}

			return $"{sign}{hoursStr}{minutesStr}:{secondsStr}{millisecondsStr}";
		}

		public static string FormatTime(double totalSeconds, int decimals = 2)
		{
			return FormatTime(TimeSpan.FromSeconds(totalSeconds), decimals);
		}

		public static string FormatVersion(Version ver)
		{
			if (ver == null)
				return null;

			var Build = ver.Build > 0 ? $".{ver.Build}" : "";
			return $"{ver.Major}.{ver.Minor}{Build}";
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

		public static int[] GetSimilarQuarterBeats(int beatIndex)
		{
			var beats = new int[4];

			var beat = beatIndex;
			for (var i = 0; i < 4; i++)
			{
				beats[i] = beat;
				beat += 4;
				if (beat > 15)
					beat -= 16;
			}

			return beats;
		}

		internal class Label
		{
			public Rect Position => positionDelegate();
			public bool Enabled
			{
				get { return enabled; }
				set
				{
					if (value)
						Enable();
					else
						Disable();
				}
			}

			public int fontSize;
			public Func<Rect> positionDelegate;
			public GUIStyle style;
			public string text;
			public bool enableOutline;
			public Color outlineColor;
			public float displayTime = -1;

			bool enabled = true;
			float timer;

			public void Toggle()
			{
				Enabled = !enabled;
			}

			public void ResetTimer()
			{
				timer = 0;
			}

			public void OnGUI(string newText = null)
			{
				if (newText != null)
					text = newText;

				style.fontSize = UI.ScaleVertical(fontSize);

				if (!enabled)
					return;

				if (displayTime > 0)
				{
					if (timer >= displayTime)
					{
						Disable();
						return;
					}
					timer += Time.deltaTime;
				}
				Draw();
			}

			void Enable()
			{
				if (!enabled)
					ResetTimer();
				enabled = true;
			}

			void Disable()
			{
				enabled = false;
			}

			void Draw()
			{
				if (enableOutline)
					DrawOutline();
				GUI.Label(Position, text, style);
			}

			void DrawOutline()
			{
				var position = Position;
				var oldColor = style.normal.textColor;
				style.normal.textColor = outlineColor;
				position.x--;
				GUI.Label(position, text, style); // left
				position.x += 2;
				GUI.Label(position, text, style); // right
				position.x--;
				position.y--;
				GUI.Label(position, text, style); // bottom
				position.y += 2;
				GUI.Label(position, text, style); // up
				position.y--;
				style.normal.textColor = oldColor;
			}
		}
	}
}
