using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SpeedrunTimerModInstaller
{
	class SteamFinder
	{
		public string SteamPath { get; private set; }
		public string[] Libraries { get; private set; }

		public string FindGameFolder(string folderName)
		{
			folderName = folderName.ToLowerInvariant();

			foreach (var library in Libraries)
			{
				foreach (var folder in Directory.EnumerateDirectories(library))
				{
					if (Path.GetFileName(folder).ToLowerInvariant() == folderName)
						return folder;
				}
			}

			return null;
		}

		public bool FindSteam()
		{
			SteamPath = null;

			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					SteamPath = FindFromRegistry();
					break;
				default:
					if (Utils.IsUnix())
					{
						SteamPath = Path.Combine(
							Environment.GetFolderPath(Environment.SpecialFolder.Personal),
							".local/share/Steam/"
						);

						if (!Directory.Exists(SteamPath))
						{
							SteamPath = Path.Combine(
								Environment.GetFolderPath(Environment.SpecialFolder.Personal),
								"Library/Application Support/Steam"
							);
						}
						break;
					}
					return false;
			}

			if (string.IsNullOrEmpty(SteamPath))
				return false;

			return FindLibraries();
		}

		bool FindLibraries()
		{
			var list = new List<string>();

			var defaultLib = Path.Combine(SteamPath, "steamapps", "common");
			if (!Directory.Exists(defaultLib))
				return false;

			list.Add(defaultLib);

			var path = Path.Combine(SteamPath, "steamapps", "libraryfolders.vdf");
			var regex = new Regex("^\\s*\"\\d+\"\\s+\"(.+)\"$");
			foreach (var line in File.ReadAllLines(path))
			{
				var match = regex.Match(line);
				if (!match.Success)
					continue;

				var libPath = match.Groups[1].Value;
				libPath = libPath.Replace("\\\\", "\\");
				list.Add(Path.Combine(libPath, "steamapps", "common"));
			}

			Libraries = list.ToArray();
			return true;
		}

		static string FindFromRegistry()
		{
			var subRegKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
			if (subRegKey != null)
				return subRegKey.GetValue("SteamPath").ToString().Replace('/', '\\');
			return null;
		}
	}
}
