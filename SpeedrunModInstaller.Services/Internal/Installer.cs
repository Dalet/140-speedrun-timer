using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SpeedrunModInstaller.Services.Exceptions;

namespace SpeedrunModInstaller.Services.Internal
{
	internal class Installer
	{
		private const string AssemblyCsharpDll = "Assembly-CSharp.dll";
		private const string SpeedrunTimerDll = "speedrun-timer.dll";
		private const string SystemCoreDll = "System.Core.dll";

		private string _gameDllBackupPath;

		private string _gameDllPath;
		private string _modDllPath;
		private string _systemCoreDllBackupPath;
		private string _systemCoreDllPath;

		public string GamePath { get; private set; }
		public string AssembliesPath { get; private set; }
		public Patcher Patcher { get; private set; }

		public Installer(string path = null)
		{
			GamePath = path;
		}

		public void SetGamePath(string path)
		{
			var success = SetGamePath(path, true);
			if (!success)
			{
				throw new InstallalationPathNotFoundException();
			}
		}

		// TODO: rewrite without side effects
		private bool SetGamePath(string path, bool recursive)
		{
			if (!string.IsNullOrEmpty(path) && Utils.IsUnix() && path.StartsWith("~/"))
			{
				var homePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				path = path.Remove(0, 2);
				path = Path.Combine(homePath, path);
			}

			string assembliesPath;
			if (!IsGamePathValid(path) || (assembliesPath = FindAssembliesPath(path)) == null)
			{
				GamePath = null;
				AssembliesPath = null;
				Patcher = null;

				// look for a .app in the directory if we didn't find anything
				if (recursive && Directory.Exists(path))
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
			_gameDllPath = Path.Combine(AssembliesPath, AssemblyCsharpDll);
			_gameDllBackupPath = Path.ChangeExtension(_gameDllPath, Path.GetExtension(_gameDllPath) + ".bak");
			_modDllPath = Path.Combine(AssembliesPath, SpeedrunTimerDll);
			_systemCoreDllPath = Path.Combine(AssembliesPath, SystemCoreDll);
			_systemCoreDllBackupPath = Path.ChangeExtension(_systemCoreDllPath, Path.GetExtension(_systemCoreDllPath) + ".bak");
			Patcher = new Patcher(AssembliesPath, _gameDllPath, _modDllPath);

			return true;
		}

		public void Uninstall()
		{
			Utils.TraceInfo("Start");

			if (File.Exists(_systemCoreDllBackupPath))
			{
				File.Delete(_systemCoreDllPath);
				File.Move(_systemCoreDllBackupPath, _systemCoreDllPath);
			}

			File.Delete(_gameDllPath);
			File.Move(_gameDllBackupPath, _gameDllPath);
			File.Delete(_modDllPath);

			Utils.TraceInfo("End");
		}

		public void Install()
		{
			Utils.TraceInfo("Start");
			Utils.Foo("C:\\Program Files (x86)\\Steam\\steamapps\\common\\140\\140_Data\\Managed\\Assembly-CSharp.dll", "zero");
			Utils.Foo("C:\\Program Files (x86)\\Steam\\steamapps\\common\\140\\140_Data\\Managed\\Assembly-CSharp.dll.tmp", "zero");

			ExtractResource(SpeedrunTimerDll, _modDllPath);
			Utils.Foo("C:\\Program Files (x86)\\Steam\\steamapps\\common\\140\\140_Data\\Managed\\Assembly-CSharp.dll", "one");
			Utils.Foo("C:\\Program Files (x86)\\Steam\\steamapps\\common\\140\\140_Data\\Managed\\Assembly-CSharp.dll.tmp", "one");

			if (File.Exists(_gameDllBackupPath))
				File.Delete(_gameDllBackupPath);

			var fileTmp = Path.Combine(AssembliesPath, Path.GetFileName(_gameDllPath) + ".tmp");
			Patcher.PatchGameDll(fileTmp);

			Utils.Foo(_gameDllPath);
			Utils.Foo(_gameDllBackupPath);
			File.Replace(fileTmp, _gameDllPath, _gameDllBackupPath);

			var tmpModSystemCore = Path.ChangeExtension(_systemCoreDllPath, Path.GetExtension(_systemCoreDllPath) + ".tmp");
			ExtractResource(SystemCoreDll, tmpModSystemCore);
			File.Replace(tmpModSystemCore, _systemCoreDllPath, _systemCoreDllBackupPath);

			Utils.TraceInfo("End");
		}

		private static void ExtractResource(string resourceName, string path)
		{
			var nameSpace = typeof(Service).Namespace;
			var manifestResName = $"{nameSpace}.Resources.{resourceName}";

			using (var dllStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResName))
			using (var fileStream = File.Create(path))
			{
				dllStream.CopyTo(fileStream);
			}
		}

		public bool IsUninstallable()
		{
			if (!IsGamePathValid())
				return false;

			return Patcher.IsGameDllPatched() && File.Exists(_gameDllBackupPath) && !Patcher.IsGameDllPatched(_gameDllBackupPath);
		}

		private bool IsGamePathValid(string path = null)
		{
			if (path == null)
				path = GamePath;

			return !string.IsNullOrEmpty(path) && Directory.Exists(path);
		}

		private static string FindAssembliesPath(string path)
		{
			if (Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar))?.EndsWith(".app") ?? false)
			{
				var assembliesPath = Path.Combine(path, "Contents", "Resources", "Data", "Managed");
				var gameDllPath = Path.Combine(assembliesPath, AssemblyCsharpDll);
				if (Directory.Exists(assembliesPath) && File.Exists(gameDllPath))
					return assembliesPath;

				assembliesPath = Path.Combine(path, "Contents", "Data", "Managed");
				gameDllPath = Path.Combine(assembliesPath, AssemblyCsharpDll);
				if (Directory.Exists(assembliesPath) && File.Exists(gameDllPath))
					return assembliesPath;
			}

			foreach (var directory in Directory.GetDirectories(path))
			{
				var gameDllPath = Path.Combine(directory, "Managed", AssemblyCsharpDll);
				if (File.Exists(gameDllPath))
					return Path.Combine(directory, "Managed");
			}

			return null;
		}
	}
}
