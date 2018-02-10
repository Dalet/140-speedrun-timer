using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpeedrunTimerMod;

namespace SpeedrunTimerModTests
{
	[TestClass]
	public class FormatTimeTests
	{
		static readonly TimeSpan HourTimeSpan = new TimeSpan(0, 1, 35, 51, 679);
		static readonly TimeSpan MinutesTimeSpan = new TimeSpan(0, 0, HourTimeSpan.Minutes,
			HourTimeSpan.Seconds, HourTimeSpan.Milliseconds);
		static readonly TimeSpan SecondsTimeSpan = new TimeSpan(0, 0, 0, HourTimeSpan.Seconds, HourTimeSpan.Milliseconds);
		static readonly TimeSpan MillisecondsTimeSpan = new TimeSpan(0, 0, 0, 0, HourTimeSpan.Milliseconds);

		[TestMethod]
		public void FormatTime_Decimals()
		{
			Assert.AreEqual("0:00", Utils.FormatTime(TimeSpan.Zero, -1));
			Assert.AreEqual("0:00", Utils.FormatTime(TimeSpan.Zero, 0));
			Assert.AreEqual("0:00.0", Utils.FormatTime(TimeSpan.Zero, 1));
			Assert.AreEqual("0:00.00", Utils.FormatTime(TimeSpan.Zero, 2));
			Assert.AreEqual("0:00.000", Utils.FormatTime(TimeSpan.Zero, 3));
			Assert.AreEqual("0:00.0000", Utils.FormatTime(TimeSpan.Zero, 4));
			Assert.AreEqual("0:00.00000", Utils.FormatTime(TimeSpan.Zero, 5));

			Assert.AreEqual("1:35:51", Utils.FormatTime(HourTimeSpan, -1));
			Assert.AreEqual("1:35:51", Utils.FormatTime(HourTimeSpan, 0));
			Assert.AreEqual("1:35:51.7", Utils.FormatTime(HourTimeSpan, 1));
			Assert.AreEqual("1:35:51.68", Utils.FormatTime(HourTimeSpan, 2));
			Assert.AreEqual("1:35:51.679", Utils.FormatTime(HourTimeSpan, 3));
			Assert.AreEqual("1:35:51.6790", Utils.FormatTime(HourTimeSpan, 4));
			Assert.AreEqual("1:35:51.67900", Utils.FormatTime(HourTimeSpan, 5));

			Assert.AreEqual("35:51", Utils.FormatTime(MinutesTimeSpan, -1));
			Assert.AreEqual("35:51", Utils.FormatTime(MinutesTimeSpan, 0));
			Assert.AreEqual("35:51.7", Utils.FormatTime(MinutesTimeSpan, 1));
			Assert.AreEqual("35:51.68", Utils.FormatTime(MinutesTimeSpan, 2));
			Assert.AreEqual("35:51.679", Utils.FormatTime(MinutesTimeSpan, 3));
			Assert.AreEqual("35:51.6790", Utils.FormatTime(MinutesTimeSpan, 4));
			Assert.AreEqual("35:51.67900", Utils.FormatTime(MinutesTimeSpan, 5));

			Assert.AreEqual("0:51", Utils.FormatTime(SecondsTimeSpan, -1));
			Assert.AreEqual("0:51", Utils.FormatTime(SecondsTimeSpan, 0));
			Assert.AreEqual("0:51.7", Utils.FormatTime(SecondsTimeSpan, 1));
			Assert.AreEqual("0:51.68", Utils.FormatTime(SecondsTimeSpan, 2));
			Assert.AreEqual("0:51.679", Utils.FormatTime(SecondsTimeSpan, 3));
			Assert.AreEqual("0:51.6790", Utils.FormatTime(SecondsTimeSpan, 4));
			Assert.AreEqual("0:51.67900", Utils.FormatTime(SecondsTimeSpan, 5));

			Assert.AreEqual("0:00", Utils.FormatTime(MillisecondsTimeSpan, -1));
			Assert.AreEqual("0:00", Utils.FormatTime(MillisecondsTimeSpan, 0));
			Assert.AreEqual("0:00.7", Utils.FormatTime(MillisecondsTimeSpan, 1));
			Assert.AreEqual("0:00.68", Utils.FormatTime(MillisecondsTimeSpan, 2));
			Assert.AreEqual("0:00.679", Utils.FormatTime(MillisecondsTimeSpan, 3));
			Assert.AreEqual("0:00.6790", Utils.FormatTime(MillisecondsTimeSpan, 4));
			Assert.AreEqual("0:00.67900", Utils.FormatTime(MillisecondsTimeSpan, 5));
		}

		[TestMethod]
		public void FormatTime_Sign()
		{
			Assert.AreEqual("1:35:51", Utils.FormatTime(HourTimeSpan, 0));
			Assert.AreEqual("-1:35:51", Utils.FormatTime(HourTimeSpan.Negate(), 0));

			Assert.AreEqual("35:51.68", Utils.FormatTime(MinutesTimeSpan, 2));
			Assert.AreEqual("-35:51.68", Utils.FormatTime(MinutesTimeSpan.Negate(), 2));

			Assert.AreEqual("0:51.679", Utils.FormatTime(SecondsTimeSpan, 3));
			Assert.AreEqual("-0:51.679", Utils.FormatTime(SecondsTimeSpan.Negate(), 3));

			Assert.AreEqual("0:00.679", Utils.FormatTime(MillisecondsTimeSpan, 3));
			Assert.AreEqual("-0:00.679", Utils.FormatTime(MillisecondsTimeSpan.Negate(), 3));
		}

		[TestMethod]
		public void FormatTime_ZeroPadding()
		{
			Assert.AreEqual("0:00.000", Utils.FormatTime(TimeSpan.Zero, 3));

			var ts = new TimeSpan(0, 1, 1, 3, 023);
			Assert.AreEqual("1:01:03.0", Utils.FormatTime(ts, 1));

			var ts2 = new TimeSpan(0, 0, 1, 3, 023);
			Assert.AreEqual("1:03.023", Utils.FormatTime(ts2, 3));

			var ts3 = new TimeSpan(0, 0, 0, 3, 023);
			Assert.AreEqual("0:03.023", Utils.FormatTime(ts3, 3));

			var ts4 = new TimeSpan(0, 0, 10, 10, 523);
			Assert.AreEqual("10:10.523", Utils.FormatTime(ts4, 3));

			var ts5 = new TimeSpan(0, 1, 10, 10, 1);
			Assert.AreEqual("1:10:10.00", Utils.FormatTime(ts5, 2));
		}

		[TestMethod]
		public void FormatTime_TimeSpanMillsecondRounded()
		{
			var ts = TimeSpan.FromTicks(23109996174); // 00:38:30.9996174
			Assert.AreEqual("38:31.000", Utils.FormatTime(ts, 3));
		}
	}
}
