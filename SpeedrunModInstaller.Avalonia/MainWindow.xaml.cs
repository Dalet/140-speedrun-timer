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

		public static readonly AvaloniaProperty<bool> InstallButtonEnabledDeclaration = AvaloniaProperty.Register<MainWindow, bool>(nameof(InstallButtonEnabled), inherits: true);
		public static readonly AvaloniaProperty<bool> UninstallButtonEnabledDeclaration = AvaloniaProperty.Register<MainWindow, bool>(nameof(UninstallButtonEnabled), inherits: true);

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

		public bool InstallButtonEnabled
		{
			get => GetValue(InstallButtonEnabledDeclaration);
			set => SetValue(InstallButtonEnabledDeclaration, value);
		}

		public bool UninstallButtonEnabled
		{
			get => GetValue(UninstallButtonEnabledDeclaration);
			set => SetValue(UninstallButtonEnabledDeclaration, value);
		}

		private readonly Service _service;

		// TODO add feedback on success/failure?

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
				UpdateButtonStatus(path);
			}

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

			UpdateButtonStatus(InstallationPath);
		}

		[UsedImplicitly]
		public void Install_Click(object sender, EventArgs args)
		{
			try
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
			catch (Exception ex)
			{
				Trace.TraceError($"Installer failed with {ex.GetType().Name}: {ex.Message}");
			}

			UpdateButtonStatus(InstallationPath);
		}

		[UsedImplicitly]
		public void Uninstall_Click(object sender, EventArgs args)
		{
			try
			{
				var settings = new Settings
				{
					Path = InstallationPath
				};

				var status = _service.Check(settings);
				switch (status)
				{
					case InstallationStatus.Installed:
					case InstallationStatus.Outdated:
						_service.Uninstall(settings);
						break;
					default:
						break;
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError($"Installer failed with {ex.GetType().Name}: {ex.Message}");
			}

			UpdateButtonStatus(InstallationPath);
		}

		[UsedImplicitly]
		public void TextBox_Event(object sender, EventArgs args)
		{
			UpdateButtonStatus(InstallationPath);
		}

		private void UpdateButtonStatus(string path)
		{
			try
			{
				var status = _service.Check(new Settings { Path = path });
				switch (status)
				{
					case InstallationStatus.ModNotInstalled:
						InstallButtonText = "Install";
						InstallButtonEnabled = true;
						UninstallButtonEnabled = false;
						break;
					case InstallationStatus.Installed:
						InstallButtonText = "Reinstall";
						InstallButtonEnabled = true;
						UninstallButtonEnabled = true;
						break;
					case InstallationStatus.Outdated:
						InstallButtonText = "Update";
						InstallButtonEnabled = true;
						UninstallButtonEnabled = true;
						break;
					case InstallationStatus.GameNotInstalled:
					case InstallationStatus.ManualInstallationDetected:
						InstallButtonText = "Unable to install..";
						InstallButtonEnabled = false;
						UninstallButtonEnabled = false;
						break;

					default:
						Trace.TraceWarning($"Unexpected installation status: {status}");
						break;
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError($"Could not update button status due to {ex.GetType()}: {ex.Message}");
			}
		}
	}
}
