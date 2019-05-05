using System.Diagnostics;
using System.Reflection;
using SpeedrunModInstaller.Services.Exceptions;
using SpeedrunModInstaller.Services.Internal;

namespace SpeedrunModInstaller.Services
{
	public class Service
	{
		public bool TryGetDefaultInstallationPath(out string path)
		{
			Utils.TraceInfo("");

			var steamFinder = new SteamFinder();
			if (steamFinder.FindSteam())
			{
				foreach (var gameFolder in steamFinder.FindGameFolders("140"))
				{
					try
					{
						var installer = new Installer();
						installer.SetGamePath(gameFolder);
						path = gameFolder;
						return true;
					}
					catch (InstallalationPathNotFoundException)
					{
						// Expected
					}
				}
			}

			path = null;
			return false;
		}

		public void Install(Settings settings)
		{
			Utils.TraceInfo(settings.Path);

			var installer = new Installer();
			installer.SetGamePath(settings.Path);

			var status = CheckInstall(installer);
			if (status == InstallationStatus.Installed)
			{
				// TODO Consider informing the user
				Trace.TraceInformation("Already installed");
				return;
			}

			installer.Install();
		}

		public void Uninstall(Settings settings)
		{
			Utils.TraceInfo(settings.Path);

			var installer = new Installer();
			installer.SetGamePath(settings.Path);

			var status = CheckInstall(installer);
			if (status != InstallationStatus.Installed && status != InstallationStatus.Outdated)
			{
				throw new UnableToUninstallException();
			}

			installer.Uninstall();
		}

		public void Reinstall(Settings settings)
		{
			Utils.TraceInfo(settings.Path);
			var installer = new Installer();
			installer.SetGamePath(settings.Path);

			var status = CheckInstall(installer);
			if (status != InstallationStatus.Installed && status != InstallationStatus.Outdated)
			{
				throw new UnableToUninstallException();
			}

			installer.Uninstall();
			installer.SetGamePath(installer.GamePath); // temp fix
			installer.Install();
		}

		public InstallationStatus Check(Settings settings)
		{
			Utils.TraceInfo(settings.Path);

			var installer = new Installer();
			try
			{
				installer.SetGamePath(settings.Path);
			}
			catch (InstallalationPathNotFoundException)
			{
				return InstallationStatus.GameNotInstalled;
			}

			return CheckInstall(installer);
		}

		private static InstallationStatus CheckInstall(Installer installer)
		{
			if (!installer.IsUninstallable())
			{
				return installer.Patcher.IsGameDllPatched()
					? InstallationStatus.ManualInstallationDetected
					: InstallationStatus.ModNotInstalled;
			}

			var installedVer = installer.Patcher.GetModDllVersion();
			var installerVer = Assembly.GetExecutingAssembly().GetName().Version;
			if (installedVer != null && installerVer > installedVer)
				return InstallationStatus.Outdated;

			return InstallationStatus.Installed;
		}
	}
}
