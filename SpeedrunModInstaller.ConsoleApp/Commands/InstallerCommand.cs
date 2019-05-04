using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mono.Options;

namespace SpeedrunModInstaller.ConsoleApp.Commands
{
	public class InstallerCommand : Command
	{
		private readonly Action _action;

		public InstallerCommand(string name, string help, Action action)
			: base(name, help)
		{
			_action = action;
			Options = new OptionSet
			{
				$"usage: <executable> {name}",
				""
			};
		}

		public override int Invoke(IEnumerable<string> arguments)
		{
			try
			{
				_action();
				return 0;
			}
			catch (Exception ex)
			{
				Trace.TraceError($"Unexpected {ex.GetType().Name} while running {Name}: {ex.Message}");
				return 1;
			}
		}
	}
}
