using System;

namespace SpeedrunTimerMod
{
	class Settings
	{
		const bool DEFAULT_LIVESPLIT_SYNC_ENABLED = true;

		public bool ModDisabled { get; private set; }
		public bool LiveSplitSyncEnabled { get; private set; } = DEFAULT_LIVESPLIT_SYNC_ENABLED;

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
				}
			}
		}
	}
}
