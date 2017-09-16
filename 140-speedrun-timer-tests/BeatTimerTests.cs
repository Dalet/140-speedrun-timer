using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpeedrunTimerMod.BeatTiming;

namespace SpeedrunTimerModTests
{
	[TestClass]
	public class BeatTimerTests
	{
		[TestMethod]
		public void BeatTimer_OnQuarterBeat()
		{
			var beatTimer = new BeatTimer(60);

			beatTimer.OnQuarterBeat();

			Assert.AreEqual(0, beatTimer.Time.Milliseconds);

			beatTimer.StartTimer();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();

			Assert.AreEqual(3 * 250, beatTimer.Time.Milliseconds);
		}

		[TestMethod]
		public void BeatTimer_StartTimer()
		{
			var beatTimer = new BeatTimer(60);

			Assert.IsFalse(beatTimer.IsStarted);

			beatTimer.StartTimer(1111, 3);
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();

			Assert.IsTrue(beatTimer.IsStarted);
			Assert.IsFalse(beatTimer.IsPaused);
			Assert.AreEqual((-3 + 2) * 250 - 1111, beatTimer.Time.Milliseconds);
			Assert.AreEqual((-3 + 2) * 250 - 1111, beatTimer.RealTime.Milliseconds);


			beatTimer = new BeatTimer(60);

			beatTimer.StartTimer(-260, 1);
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();

			Assert.AreEqual((3 - 1) * 250 + 260, beatTimer.Time.Milliseconds);
			Assert.AreEqual((3 - 1) * 250 + 260, beatTimer.RealTime.Milliseconds);
		}

		[TestMethod]
		public void BeatTimer_PauseTimer()
		{
			var beatTimer = new BeatTimer(60);

			beatTimer.StartTimer();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.PauseTimer();
			Assert.IsTrue(beatTimer.IsPaused);
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();

			Assert.AreEqual(2 * 250, beatTimer.Time.Milliseconds);
			Assert.AreEqual(5 * 250, beatTimer.RealTime.Milliseconds);


			beatTimer = new BeatTimer(60);

			beatTimer.StartTimer();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.PauseTimer(1111, -3);
			beatTimer.OnQuarterBeat();

			Assert.AreEqual((2 - 3) * 250 + 1111, beatTimer.Time.Milliseconds);
			Assert.AreEqual(3 * 250, beatTimer.RealTime.Milliseconds);
		}

		[TestMethod]
		public void BeatTimer_ResumeTimer()
		{

			var beatTimer = new BeatTimer(60);

			beatTimer.StartTimer();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.PauseTimer();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.ResumeTimer();
			beatTimer.OnQuarterBeat();

			Assert.AreEqual(3 * 250, beatTimer.Time.Milliseconds);
			Assert.AreEqual(5 * 250, beatTimer.RealTime.Milliseconds);


			beatTimer = new BeatTimer(60);

			beatTimer.StartTimer();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.PauseTimer(1111, -3);
			beatTimer.OnQuarterBeat();
			beatTimer.ResumeTimer(42, 8);

			Assert.AreEqual(250 - (1111 - 3 * 250) + 42 + 8 * 250, beatTimer.TimePaused.Milliseconds);
			Assert.AreEqual((2 - 3 - 8) * 250 + 1111 - 42, beatTimer.Time.Milliseconds);
			Assert.AreEqual(3 * 250, beatTimer.RealTime.Milliseconds);
		}

		[TestMethod]
		public void BeatTimer_AddRealTime()
		{
			var beatTimer = new BeatTimer(60);

			beatTimer.StartTimer();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.OnQuarterBeat();
			beatTimer.AddRealTime(11);

			Assert.AreEqual(4 * 250 + 11, beatTimer.RealTime.Milliseconds);
			Assert.AreEqual(4 * 250, beatTimer.Time.Milliseconds);
		}
	}
}
