using System;

namespace SpeedrunTimerMod
{
	class Settings
	{
		const bool DEFAULT_LIVESPLIT_SYNC_ENABLED = true;

		public bool LiveSplitSyncEnabled { get; set; } = DEFAULT_LIVESPLIT_SYNC_ENABLED;

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
					case "-disable-livesplit-sync":
						LiveSplitSyncEnabled = false;
						break;
				}
			}
		}
	}
}
