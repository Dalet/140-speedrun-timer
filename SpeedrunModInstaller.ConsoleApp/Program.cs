using System;
using System.Diagnostics;
using Mono.Options;
using SpeedrunModInstaller.ConsoleApp.Commands;
using SpeedrunModInstaller.Services;

namespace SpeedrunModInstaller.ConsoleApp
{
	internal class Program
	{
		public static int Main(string[] args)
		{
			var service = new Service();
			var settings = new Settings();

			var commands = new CommandSet("commands")
			{
				"usage: <executable> COMMAND [OPTIONS]",
				"",
				"Installer for 140-speedrun-timer",
				"",
				"Global options:",
				{"p|path=", "Manually specify where 140 is installed", v => settings.Path = v},
				{"v|verbose", "Verbose", v => SetVerbose()},
				"",
				"Available commands:",
				new InstallerCommand("install", "Install the mod", () => service.Install(settings)),
				new InstallerCommand("reinstall", "Reinstall the mod", () => service.Reinstall(settings)),
				new InstallerCommand("uninstall", "Uninstall the mod", () => service.Uninstall(settings)),
				new InstallerCommand("check", "Check whether the mod is installed", () => Check(service, settings)),
				new InstallerCommand("find", "Attempts to find the path where 140 is installed", () => Find(service))
			};

			int exitCode;
			try
			{
				exitCode = commands.Run(args);
			}
			catch (Exception ex)
			{
				exitCode = 1;
				Trace.TraceError($"Unexpected {ex.GetType()}: {ex.Message}");
			}
			finally
			{
				Trace.Flush();
			}

			return exitCode;
		}

		private static void SetVerbose()
		{
			var traceListener = new TextWriterTraceListener(Console.Out);
			Trace.Listeners.Add(traceListener);
		}

		private static void Check(Service service, Settings settings)
		{
			var status = service.Check(settings);
			switch (status)
			{
				case InstallationStatus.ModNotInstalled:
					Console.WriteLine("Mod is NOT installed");
					break;
				case InstallationStatus.Installed:
					Console.WriteLine("Mod is installed and is up to date");
					break;
				case InstallationStatus.ManualInstallationDetected:
					Console.WriteLine("The game has been manually modified. Please reinstall the game, then try again.");
					break;
				case InstallationStatus.Outdated:
					Console.WriteLine("Mod is installed, but OUTDATED");
					break;
				case InstallationStatus.GameNotInstalled:
					Console.WriteLine("The game is not installed at the specified location");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static void Find(Service service)
		{
			if (!service.TryGetDefaultInstallationPath(out var path))
			{
				Console.WriteLine("Could not find the installation path");
			}

			Console.WriteLine(path);
		}
	}
}
