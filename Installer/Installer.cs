using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SpeedrunTimerModInstaller
{
	class Installer
	{
		public string GamePath { get; private set; }
		public string AssembliesPath { get; private set; }
		public Patcher Patcher { get; private set; }

		string _gameDllPath;
		string _gameDllBackupPath;
		string _modDllPath;

		public Installer(string path = null)
		{
			GamePath = path;
		}

		public bool SetGamePath(string path) => SetGamePath(path, true);

		bool SetGamePath(string path, bool recursive)
		{
			if (!string.IsNullOrEmpty(path) && Utils.IsUnix() && path.StartsWith("~/"))
			{
				var homePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				path = path.Remove(0, 2);
				path = Path.Combine(homePath, path);
			}

			string assembliesPath;
			if (!isGamePathValid(path) || (assembliesPath = FindAssembliesPath(path)) == null)
			{
				GamePath = null;
				AssembliesPath = null;
				Patcher = null;

				// look for a .app in the directory if we didn't find anything
				if (recursive && Utils.IsUnix() && Directory.Exists(path))
				{
					var dir = Directory.EnumerateDirectories(path)
						.FirstOrDefault(d => Path.GetFileName(d.TrimEnd(Path.DirectorySeparatorChar)).EndsWith(".app"));
					if (dir != null)
						return SetGamePath(dir, false);
				}
				return false;
			}

			GamePath = path;
			AssembliesPath = assembliesPath;
			_gameDllPath = Path.Combine(AssembliesPath, "Assembly-CSharp.dll");
			_gameDllBackupPath = Path.ChangeExtension(_gameDllPath, Path.GetExtension(_gameDllPath) + ".bak");
			_modDllPath = Path.Combine(AssembliesPath, "speedrun-timer.dll");
			Patcher = new Patcher(AssembliesPath, _gameDllPath, _modDllPath);

			return true;
		}

		public void UnInstall()
		{
			File.Delete(_gameDllPath);
			File.Move(_gameDllBackupPath, _gameDllPath);
			File.Delete(_modDllPath);
		}

		public void Install()
		{
			ExtractModDll(_modDllPath);

			if (File.Exists(_gameDllBackupPath))
				File.Delete(_gameDllBackupPath);

			var fileTmp = Path.Combine(AssembliesPath, Path.GetFileName(_gameDllPath) + ".tmp");
			Patcher.PatchGameDll(fileTmp);
			File.Replace(fileTmp, _gameDllPath, _gameDllBackupPath);
		}

		static void ExtractModDll(string path)
		{
			var nameSpace = typeof(Program).Namespace;
			var resFolder = "Resources";
			var fileName = "speedrun-timer.dll";
			var manifestResName = $"{nameSpace}.{resFolder}.{fileName}";

			using (var dllStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResName))
			using (var fileStream = File.Create(path))
			{
				dllStream.CopyTo(fileStream);
			}
		}

		public bool IsUninstallable()
		{
			if (!isGamePathValid())
				return false;

			if (File.Exists(_gameDllBackupPath))
				return true;
			return false;
		}

		bool isGamePathValid(string path = null)
		{
			if (path == null)
				path = GamePath;
			return !string.IsNullOrEmpty(path) && Directory.Exists(path);
		}

		static string FindAssembliesPath(string path)
		{
			if (Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar))?.EndsWith(".app") ?? false)
			{
				var assembliesPath = Path.Combine(path, "Contents", "Data", "Managed");
				var gameDllPath = Path.Combine(assembliesPath, "Assembly-CSharp.dll");
				if (Directory.Exists(assembliesPath) && File.Exists(gameDllPath))
					return assembliesPath;
			}

			foreach (var directory in Directory.GetDirectories(path))
			{
				var gameDllPath = Path.Combine(directory, "Managed", "Assembly-CSharp.dll");
				if (File.Exists(gameDllPath))
					return Path.Combine(directory, "Managed");
			}

			return null;
		}
	}
}
