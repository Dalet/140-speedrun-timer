using System;

namespace SpeedrunTimerMod
{
	class Settings
	{
		const bool DEFAULT_LIVESPLIT_SYNC_ENABLED = true;
		const int DEFAULT_TARGET_FRAMERATE = -1;
		const bool DEFAULT_VSYNC = true;
		const bool DEFAULT_RUN_BACKGROUND = false;

		public bool ModDisabled { get; private set; }
		public bool LiveSplitSyncEnabled { get; private set; } = DEFAULT_LIVESPLIT_SYNC_ENABLED;
		public bool FlashCheatWatermark { get; private set; }
		public int TargetFramerate { get; set; } = DEFAULT_TARGET_FRAMERATE;
		public bool Vsync { get; set; } = DEFAULT_VSYNC;
		public bool RunInBackground { get; set; } = DEFAULT_RUN_BACKGROUND;

		public bool Load()
		{
			ReadCommandLineSettings();
			return true;
		}

		void ReadCommandLineSettings()
		{
			var args = Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "-disable-timer-mod":
						ModDisabled = true;
						break;
					case "-disable-livesplit-sync":
						LiveSplitSyncEnabled = false;
						break;
					case "-flash-cheat-watermark":
						FlashCheatWatermark = true;
						break;
				}
			}
		}
	}
}
