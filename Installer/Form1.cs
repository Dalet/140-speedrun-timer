using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SpeedrunTimerModInstaller
{
	public partial class Form1 : Form
	{
		Installer _installer;
		SteamFinder _steamFinder;
		bool _needPathValidation;

		public Form1()
		{
			InitializeComponent();

			_installer = new Installer();
			_steamFinder = new SteamFinder();
			Text = Text + $" v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

			if (_steamFinder.FindSteam())
			{
				var gameFolder = _steamFinder.FindGameFolder("140");
				if (!string.IsNullOrEmpty(gameFolder))
					txtGamePath.Text = gameFolder;
			}
			ValidatePath();
		}

		void btnInstall_Click(object sender, EventArgs e)
		{
			btnInstall.Enabled = false;

			if (_needPathValidation)
			{
				ValidatePath();
				return;
			}

			if (_installer.GamePath == null)
			{
				MessageBox.Show(this, "The game folder is invalid.\n", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			_needPathValidation = true;

#if !DEBUG
			try
			{
#endif
				var uninstall = _installer.IsUninstallable();
				if (uninstall)
					_installer.UnInstall();
				else if (!_installer.Patcher.IsGameDllPatched())
				{
					_installer.Install();
				}
				else
				{
					MessageBox.Show(this, "A manual installation of the mod was detected.\n"
						+ "This installer requires the game files to be clean. Verify the game files in Steam before trying again.\n",
						"Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				var word = uninstall ? "Uninstall" : "Install";
				MessageBox.Show(this, $"{word}ation success.\n", $"{word}er",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
#if !DEBUG
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, $"An error occured:\n\n{ex.Message}\n", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
#endif
			ValidatePath();
		}

		void txtGamePath_TextChanged(object sender, EventArgs e)
		{
			_needPathValidation = true;
			btnInstall.Text = "Scan folder";
			btnInstall.Enabled = true;
		}

		void ValidatePath()
		{
			var path = txtGamePath.Text.Trim();
			var isValid = _installer.SetGamePath(path);
			btnInstall.Enabled = isValid;
			btnInstall.Text = _installer.IsUninstallable() ? "Uninstall" : "Install";
			_needPathValidation = false;
		}

		static string ShowOpenFileDialog(string path = null)
		{
			using (var fileDialog = new FolderBrowserDialog()
			{
				SelectedPath = path ?? "",
				ShowNewFolderButton = false,
				RootFolder = Environment.SpecialFolder.MyComputer
			})
			{
				SendKeys.Send("{TAB}{TAB}{RIGHT}"); // http://stackoverflow.com/questions/6942150/why-folderbrowserdialog-dialog-does-not-scroll-to-selected-folder
				var result = fileDialog.ShowDialog();
				if (result == DialogResult.OK)
					path = fileDialog.SelectedPath;
				if (Directory.Exists(fileDialog.SelectedPath))
					return path;
				else
					return "";
			}
		}

		void btnBrowse_Click(object sender, EventArgs e)
		{
			var path = txtGamePath.Text.Trim();
			if (string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(_steamFinder.SteamPath))
				path = Path.Combine(_steamFinder.SteamPath, "steamapps", "common");
			txtGamePath.Text = ShowOpenFileDialog(path);
			ValidatePath();
		}
	}
}
