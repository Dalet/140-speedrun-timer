using System;

namespace SpeedrunTimerModInstaller
{
	static class Program
	{
		enum ExitCode : int
		{
			Success,
			Error,
			NoArgs,
			InvalidPath,
			NotInstalled,
			ManualInstallationDetected,
			AlreadyDone,
			PermissionError
		}

		static Installer _installer;

		[STAThread]
		static int Main(string[] args)
		{
			var exitCode = ExitCode.NoArgs;

			try
			{
				for (int i = 0; i < args.Length; i++)
				{
					switch (args[i])
					{
						case "--find-game":
							exitCode = FindGame();
							break;
						case "--install":
							exitCode = Install(GetNextArg(args, i));
							break;
						case "--uninstall":
							exitCode = Uninstall(GetNextArg(args, i));
							break;
						case "--check-install":
							exitCode = CheckInstall(GetNextArg(args, i));
							break;
						default:
							break;
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
				return (int)ExitCode.PermissionError;
			}

			return (int)exitCode;
		}

		static string GetNextArg(string[] args, int currentArgIndex)
		{
			if (currentArgIndex < args.Length - 1)
				return args[currentArgIndex + 1];
			Console.Error.WriteLine("Missing argument for " + args[currentArgIndex]);
			Environment.Exit((int)ExitCode.Error);
			return null;
		}

		static bool Init(string path)
		{
			_installer = new Installer();
			return _installer.SetGamePath(path);
		}

		static ExitCode CheckInstall(string path)
		{
			var isValid = false;
			try
			{
				isValid = Init(path);
			}
			catch
			{
				Environment.Exit((int)ExitCode.Error);
			}

			if (!isValid)
			{
				Console.Error.WriteLine("Invalid install path");
				return ExitCode.InvalidPath;
			}

			if (_installer.IsUninstallable())
				return ExitCode.Success;
			else if (_installer.Patcher.IsGameDllPatched())
				return ExitCode.ManualInstallationDetected;
			else
				return ExitCode.NotInstalled;
		}

		static ExitCode FindGame()
		{
			var steamFinder = new SteamFinder();

			if (steamFinder.FindSteam())
			{
				var gameFolder = steamFinder.FindGameFolder("140");
				var installer = new Installer();
				if (installer.SetGamePath(gameFolder))
				{
					Console.WriteLine(gameFolder);
					return ExitCode.Success;
				}
			}

			Console.Error.WriteLine("Did not find the game folder");
			return ExitCode.Error;
		}

		static ExitCode Install(string path) => Install(path, true);
		static ExitCode Uninstall(string path) => Install(path, false);

		static ExitCode Install(string path, bool install)
		{
			if (!Init(path))
			{
				Console.Error.WriteLine("Invalid install path");
				return ExitCode.InvalidPath;
			}

			var unInstallable = _installer.IsUninstallable();

			if (install)
			{
				if (unInstallable)
					return ExitCode.AlreadyDone;
				_installer.Install();
			}
			else
			{
				if (!unInstallable && _installer.Patcher.IsGameDllPatched())
					return ExitCode.ManualInstallationDetected;
				_installer.UnInstall();
			}

			return ExitCode.Success;
		}
	}
}
