using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpeedrunTimerMod.BeatTiming;

namespace SpeedrunTimerModTests
{
	[TestClass]
	public class BeatTimeTests
	{
		[TestMethod]
		public void BeatTime_OperatorAdd()
		{
			var left = new BeatTime(140, 70, 600);
			var right = new BeatTime(140, 20, -310);
			var zeroBpmLeft = new BeatTime(0, 4, 100);
			var zeroBpmRight = new BeatTime(0, 28, -300);

			{
				var result = left + right;

				Assert.AreEqual(140, result.bpm);
				Assert.AreEqual(90, result.quarterBeatCount);
				Assert.AreEqual(290, result.offset);
			}
			{
				var result = left + zeroBpmRight;

				Assert.AreEqual(left.TimeSpan, result.TimeSpan);
				Assert.AreEqual(left.bpm, result.bpm);
			}
			{
				var result = zeroBpmLeft + right;

				Assert.AreEqual(right.TimeSpan, result.TimeSpan);
				Assert.AreEqual(right.bpm, result.bpm);
			}
			{
				var result = zeroBpmLeft + zeroBpmRight;

				Assert.AreEqual(32, result.quarterBeatCount);
				Assert.AreEqual(-200, result.offset);
				Assert.AreEqual(0, result.bpm);
			}
		}

		[TestMethod]
		public void BeatTime_OperatorSub()
		{
			var left = new BeatTime(140, 70, 600);
			var right = new BeatTime(140, 20, -310);
			var zeroBpmLeft = new BeatTime(0, 4, 100);
			var zeroBpmRight = new BeatTime(0, 28, -300);

			{
				var result = left - right;

				Assert.AreEqual(140, result.bpm);
				Assert.AreEqual(50, result.quarterBeatCount);
				Assert.AreEqual(910, result.offset);
			}
			{
				var result = zeroBpmLeft - right;

				Assert.AreEqual(right.TimeSpan.Negate(), result.TimeSpan);
				Assert.AreEqual(right.bpm, result.bpm);
			}
			{
				var result = zeroBpmLeft - zeroBpmRight;

				Assert.AreEqual(-24, result.quarterBeatCount);
				Assert.AreEqual(400, result.offset);
				Assert.AreEqual(0, result.bpm);
			}
			{
				var result = left - zeroBpmRight;

				Assert.AreEqual(left.TimeSpan, result.TimeSpan);
				Assert.AreEqual(left.bpm, result.bpm);
			}
		}

		[TestMethod]
		public void BeatTime_InvalidDifferentBpmOperations()
		{
			var left = new BeatTime(50, 70, 600);
			var right = new BeatTime(140, 20, -310);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var result = left + right;
			});

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var result = left - right;
			});
		}

		[TestMethod]
		public void BeatTime_Milliseconds()
		{
			var beatTime = new BeatTime(60, 4, 111);

			var result = beatTime.Milliseconds;

			Assert.AreEqual(1111, result);
		}

		[TestMethod]
		public void BeatTime_AddQuarterBeats()
		{
			var beatTime = new BeatTime(11, 18, 1);

			var result = beatTime.AddQuarterBeats(3);
			var result2 = beatTime.AddQuarterBeats(-25);

			Assert.AreEqual(21, result.quarterBeatCount);
			Assert.AreEqual(1, result.offset);
			Assert.AreEqual(-7, result2.quarterBeatCount);
			Assert.AreEqual(18, beatTime.quarterBeatCount, "BeatTime should be immutable!");
		}

		[TestMethod]
		public void BeatTime_AddOffset()
		{
			var beatTime = new BeatTime(11, 18, 1);

			var result = beatTime.AddOffset(-7);

			Assert.AreEqual(-6, result.offset);
			Assert.AreEqual(1, beatTime.offset, "BeatTime should be immutable!");
		}

		[TestMethod]
		public void BeatTime_DefaultValue()
		{
			var beatTime = new BeatTime();

			Assert.AreEqual(0, beatTime.quarterBeatCount);
			Assert.AreEqual(0, beatTime.offset);
			Assert.AreEqual(TimeSpan.Zero, beatTime.TimeSpan);
			Assert.AreEqual(0, beatTime.Milliseconds);
		}
	}
}
