using UnityEngine;

namespace SpeedrunTimerMod
{
	internal sealed class Misc : MonoBehaviour
	{
		float _resetTimer;
		float _resetHits;

		static int _beatIndex = -1;
		public static int BeatIndex
		{
			get { return _beatIndex; }
			private set
			{
				_beatIndex = value;
				MakeDebugString();
			}
		}

		public static string BeatDbgStr { get; private set; }

		public void Start()
		{
			BeatIndex = -1;
			Globals.beatMaster.onBeat += OnBeat;
			Globals.beatMaster.onBeatReset += OnBeatReset;
		}

		public void Update()
		{
			if (Application.isLoadingLevel)
				return;

			if (_resetHits == 1)
				_resetTimer += Time.deltaTime;

			if (_resetTimer > 0.5f)
				_resetHits = 0;

			if (Input.GetKeyDown(KeyCode.R))
			{
				if (_resetHits == 0)
					_resetTimer = 0;

				_resetHits++;
				if (_resetHits > 1 && _resetTimer < 0.5f)
				{
					_resetHits = 0;
					SpeedrunTimer.Instance.ResetTimer();
					MirrorModeManager.mirrorModeActive = false;
					MirrorModeManager.respawnFromMirror = false;
					var cheatComponent = ModLoader.ModLevelObject.GetComponent<Cheats>();
					cheatComponent.FlashWatermarkAcrossLoad();
					Application.LoadLevel("Level_Menu");
				}
			}
		}

		void OnBeat(int index)
		{
			BeatIndex = index;
		}

		void OnBeatReset()
		{
			BeatIndex = -1;
		}

		static void MakeDebugString()
		{
			if (_beatIndex < 0)
			{
				BeatDbgStr = "-";
				return;
			}

			var beat = _beatIndex + 1;
			BeatDbgStr = $"{beat.ToString().PadLeft(2, '0')}/16";
		}
	}
}
