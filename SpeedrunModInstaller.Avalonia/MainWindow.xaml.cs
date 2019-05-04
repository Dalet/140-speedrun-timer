using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;
using SpeedrunModInstaller.Services;

namespace SpeedrunModInstaller.Avalonia
{
	public class MainWindow : Window
	{
		public static readonly AvaloniaProperty<string> InstallationPathDeclaration = AvaloniaProperty.Register<MainWindow, string>(nameof(InstallationPath), inherits: true);
		public static readonly AvaloniaProperty<string> InstallButtonTextDeclaration = AvaloniaProperty.Register<MainWindow, string>(nameof(InstallButtonText), inherits: true);

		private readonly Service _service;

		public string InstallationPath
		{
			get => GetValue(InstallationPathDeclaration);
			set => SetValue(InstallationPathDeclaration, value);
		}

		public string InstallButtonText
		{
			get => GetValue(InstallButtonTextDeclaration);
			set => SetValue(InstallButtonTextDeclaration, value);
		}

		public MainWindow()
		{
			_service = new Service();
			InitializeComponent();
			DataContext = this;
		}

		private void InitializeComponent()
		{
			if (_service.TryGetDefaultInstallationPath(out var path))
			{
				InstallationPath = path;
			}

			var status = _service.Check(new Settings { Path = path });
			SetInstallButtonText(status);

			AvaloniaXamlLoader.Load(this);
		}

		[UsedImplicitly]
		public async void Browse_Click(object sender, EventArgs args)
		{
			var openFileDialog = new OpenFolderDialog
			{
				Title = "140 installation folder"
			};

			if (Directory.Exists(InstallationPath))
			{
				openFileDialog.InitialDirectory = InstallationPath;
			}

			var path = await openFileDialog.ShowAsync(this);
			if (!string.IsNullOrWhiteSpace(path))
			{
				InstallationPath = path;
			}
		}

		[UsedImplicitly]
		public void Install_Click(object sender, EventArgs args)
		{
			try
			{
				RunInstaller();
			}
			catch (Exception ex)
			{
				Trace.TraceError($"Installer failed with {ex.GetType()}: {ex.Message}");
			}
		}

		private void RunInstaller()
		{
			var settings = new Settings
			{
				Path = InstallationPath
			};

			var status = _service.Check(settings);
			switch (status)
			{
				case InstallationStatus.ModNotInstalled:
					_service.Install(settings);
					break;
				case InstallationStatus.Installed:
				case InstallationStatus.Outdated:
					_service.Reinstall(settings);
					break;
				default:
					break;
			}
		}

		private void SetInstallButtonText(InstallationStatus status)
		{
			switch (status)
			{
				case InstallationStatus.ModNotInstalled:
					InstallButtonText = "Install";
					break;
				case InstallationStatus.Installed:
					InstallButtonText = "Reinstall";
					break;
				case InstallationStatus.Outdated:
					InstallButtonText = "Update";
					break;
				case InstallationStatus.GameNotInstalled:
				case InstallationStatus.ManualInstallationDetected:
					InstallButtonText = "Unable to install..";
					break;

				default:
					Trace.TraceWarning($"Unexpected installation status: {status}");
					break;
			}
		}
	}
}
