using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Logging.Serilog;

namespace SpeedrunModInstaller.Avalonia
{
	internal class Program
	{
		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		private static void Main(string[] args)
		{
			Directory.CreateDirectory("logs");
			using (var writer = File.CreateText($"logs/{DateTime.Now:yyyyMMdd_HHmmssfff}.txt"))
			{
				var fileWriterListener = new TextWriterTraceListener(writer);
				Trace.Listeners.Add(fileWriterListener);
				Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
				Trace.AutoFlush = true;

				BuildAvaloniaApp().Start(AppMain, args);

				Trace.Flush();
				Trace.Listeners.Remove(fileWriterListener);
			}
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp()
		{
			return AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.LogToDebug();
		}

		// Your application's entry point. Here you can initialize your MVVM framework, DI
		// container, etc.
		private static void AppMain(Application app, string[] args)
		{
			app.Run(new MainWindow());
		}
	}
}
