using System;
using System.Diagnostics;
using System.Threading;

namespace SpeedrunTimerMod
{
	public class LiveSplitSync : IDisposable
	{
		const string PIPE_NAME = "LiveSplit";

		public event EventHandler Connected;

		public bool IsConnected => _pipeClient.IsConnected;
		public bool IsConnecting => _pipeClient.IsConnecting;

		NamedPipeClient _pipeClient;
		Stopwatch _lastTimeUpdate;

		public LiveSplitSync()
		{
			_pipeClient = new NamedPipeClient(PIPE_NAME)
			{
				AutoReconnect = true
			};
			_pipeClient.Connected += _pipe_Connected;
			_lastTimeUpdate = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			_pipeClient.Disconnect();
			_pipeClient.Dispose();
		}

		public void GracefulDispose()
		{
			_pipeClient.Disconnected += DisposeAfterGracefulDisconnect;
			GracefulDisconnect();
		}

		void DisposeAfterGracefulDisconnect(object sender, EventArgs e)
		{
			_pipeClient.Disconnected -= DisposeAfterGracefulDisconnect;
			Dispose();
		}

		void _pipe_Connected(object sender, EventArgs e)
		{
			Connected?.Invoke(this, EventArgs.Empty);
		}

		public bool Connect(int timeout = Timeout.Infinite)
		{
			return _pipeClient.Connect(timeout);
		}

		public void ConnectAsync()
		{
			_pipeClient.ConnectAsync();
		}

		public void GracefulDisconnect()
		{
			if (!IsConnected)
				return;
			_pipeClient.WriteAsync(Commands.UnPauseGameTime, (success) => _pipeClient.Disconnect());
		}

		public void Disconnect()
		{
			_pipeClient.Disconnect();
		}

		public void SendCommand(string command)
		{
			if (!IsConnected)
				return;

			if (!command.EndsWith("\n"))
				command += "\n";

			_pipeClient.WriteAsync(command);
		}

		public void Start()
		{
			var cmd = Commands.StartTimer + "\n" + Commands.AlwaysPauseGameTime;
			SendCommand(cmd);
		}

		public void UpdateTime(TimeSpan timespan, bool force = false)
		{
			if (!IsConnected || (!force && _lastTimeUpdate.ElapsedMilliseconds < 15))
				return;

			var cmd = Commands.SetGameTime + " " + Utils.FormatTime(timespan.TotalSeconds, 3);
			SendCommand(cmd);
			_lastTimeUpdate.Reset();
			_lastTimeUpdate.Start();
		}

		public void Reset()
		{
			SendCommand(Commands.Reset);
		}

		public void Split()
		{
			SendCommand(Commands.Split);
		}

		static class Commands
		{
			public const string AlwaysPauseGameTime = "alwayspausegametime";
			public const string GetBestPossibleTime = "getbestpossibletime";
			public const string GetComparisonSplitTime = "getcomparisonsplittime";
			public const string GetCurrentSplitName = "getcurrentsplitname";
			public const string GetCurrentTime = "getcurrenttime";
			public const string GetCurrentTimerPhase = "getcurrenttimerphase";
			public const string GetDelta = "getdelta";
			public const string GetFinalTime = "getfinaltime";
			public const string GetLastSplitTime = "getlastsplittime";
			public const string GetPredictedTime = "getpredictedtime";
			public const string GetPreviousSplitName = "getprevioussplitname";
			public const string GetSplitIndex = "getsplitindex";
			public const string Pause = "pause";
			public const string PauseGameTime = "pausegametime";
			public const string Reset = "reset";
			public const string Resume = "resume";
			public const string SetComparison = "setcomparison";
			public const string SetCurrentSplitName = "setcurrentsplitname";
			public const string SetGameTime = "setgametime";
			public const string SetLoadingTimes = "setloadingtimes";
			public const string SetSplitName = "setsplitname";
			public const string SkipSplit = "skipsplit";
			public const string Split = "split";
			public const string StartOrSplit = "startorsplit";
			public const string StartTimer = "starttimer";
			public const string SwitchToGameTime = "switchto gametime";
			public const string SwitchToRealTime = "switchto realtime";
			public const string UnPauseGameTime = "unpausegametime";
			public const string UnSplit = "unsplit";
		}
	}
}
