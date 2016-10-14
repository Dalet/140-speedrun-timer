using System;
using System.Windows.Forms;

namespace SpeedrunTimerModInstaller
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var form = new Form1();
#if !DEBUG
			try
			{
#endif
				Application.Run(form);
#if !DEBUG
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occured:\n\n{ex.Message}\n", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
#endif
		}
	}
}
