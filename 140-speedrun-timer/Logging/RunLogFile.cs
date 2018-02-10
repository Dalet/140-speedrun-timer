using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace SpeedrunTimerMod.Logging
{
	static class RunLogFile
	{
		public const string FILE_NAME = "speedrun-log.csv";
		public static readonly string FILE_PATH = Path.Combine(GetFolder(), FILE_NAME);

		static readonly object _syncRoot = new object();
		static FileStream _logFile;

		static string GetFolder()
		{
			if (Application.platform == RuntimePlatform.WindowsPlayer)
				return Application.dataPath;
			else
				return Application.persistentDataPath;
		}

		public static bool OpenFile()
		{
			lock (_syncRoot)
			{
				if (_logFile != null)
					return true;

				try
				{
					if (File.Exists(FILE_PATH))
					{
						new FileInfo(FILE_PATH)
						{
							IsReadOnly = false
						};
					}

					_logFile = new FileStream(FILE_PATH, FileMode.Append, FileAccess.Write, FileShare.Read);

					new FileInfo(FILE_PATH)
					{
						IsReadOnly = true
					};

					Debug.Log("Opened " + FILE_PATH);
					return true;
				}
				catch (Exception e)
				{
					Debug.Log($"Error when opening {FILE_PATH}\n{e}");

					var msg = "Error when opening " + Path.GetFileName(FILE_PATH)
						+ "\nPlease close every application that is using it.";
					ModLoader.ShowErrorMessage(msg, TimeSpan.FromSeconds(30));
					return false;
				}
			}
		}

		public static void OpenFileAsync()
		{
			var thread = new Thread(() => OpenFile());
			thread.Start();
		}

		public static void CloseFile()
		{
			lock (_syncRoot)
			{
				if (_logFile == null)
					return;

				_logFile.Dispose();
				_logFile = null;
			}
		}

		public static void WriteLineAsync(string str)
		{
			var thread = new Thread(() => WriteLine(str));
			thread.Start();
		}

		public static void WriteLine(string str)
		{
			try
			{
				lock (_syncRoot)
				{
					if (OpenFile())
					{
						var sr = new StreamWriter(_logFile);
						sr.WriteLine(str);
						sr.Flush();
					}
				}
			}
			catch (IOException e)
			{
				Debug.Log($"Run log write failed!\n{e}");
			}
		}
	}
}
