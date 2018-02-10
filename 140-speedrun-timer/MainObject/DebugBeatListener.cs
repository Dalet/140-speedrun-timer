using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SpeedrunTimerMod
{
	class DebugBeatListener : MonoBehaviour
	{
		public static int LastBeatFrame { get; private set; }
		public static int LastBeatResetFrame { get; private set; }
		public static float LastBeatTimestamp { get; private set; }
		public static float LastBeatResetTimestamp { get; private set; }
		public static float TimeSinceLastBeat => Time.time - LastBeatTimestamp;
		public static int LastBeatId { get; private set; } = -1;

		public static string DebugStr =>
			$"Frame {Time.renderedFrameCount} | Time: {Time.time} :"
			+ $" Beat #{LastBeatId} frames since last beat: {Time.renderedFrameCount - LastBeatFrame},"
			+ $" time: {TimeSinceLastBeat * 1000}ms, sw: {_sw.ElapsedMilliseconds}ms";

		static DebugBeatListener _instance;
		static Stopwatch _sw = new Stopwatch();

		bool _firstBeat;

		void Awake()
		{
			if (_instance != null)
			{
				Destroy(this);
				return;
			}

			_instance = this;
		}

		void Start()
		{
			SubscribeGlobalBeatMaster();
		}

		void OnEnable()
		{
			if (Globals.beatMaster != null)
				SubscribeGlobalBeatMaster();
		}

		void OnDisable()
		{
			UnsubGlobalBeatMaster();
		}

		void OnLevelWasLoaded(int level)
		{
			LastBeatId = -1;
			SubscribeGlobalBeatMaster();
		}

		void OnDestroy()
		{
			if (_instance != this)
				return;

			_instance = null;
			UnsubGlobalBeatMaster();
		}

		void UnsubGlobalBeatMaster()
		{
			Globals.beatMaster.onBeat -= BeatMaster_onBeat;
			Globals.beatMaster.onBeatReset -= BeatMaster_onBeatReset;
		}

		void SubscribeGlobalBeatMaster()
		{
			UnsubGlobalBeatMaster();
			Globals.beatMaster.onBeat += BeatMaster_onBeat;
			Globals.beatMaster.onBeatReset += BeatMaster_onBeatReset;
		}

		void BeatMaster_onBeatReset()
		{
			Debug.Log($"Frame {Time.renderedFrameCount} | Time: {Time.time} : Beat reset at beat #{LastBeatId}, frames since last beat: {Time.renderedFrameCount - LastBeatFrame}, time: {TimeSinceLastBeat * 1000}ms");
			LastBeatResetFrame = Time.renderedFrameCount;
			LastBeatResetTimestamp = Time.time;
			LastBeatId = -1;
			_firstBeat = true;
		}

		void BeatMaster_onBeat(int beatId)
		{
			if (_firstBeat)
			{
				_firstBeat = false;
				Debug.Log("First beat after reset:" + DebugStr);
				return;
			}

			LastBeatId = beatId;
			_sw.Reset();
			_sw.Start();
			LastBeatFrame = Time.renderedFrameCount;
			LastBeatTimestamp = Time.time;
		}
	}
}
