﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace SpeedrunTimerMod
{
	class NamedPipeClient : IDisposable
	{
		public event EventHandler Connected;
		public event EventHandler Disconnected;

		public bool IsConnecting => _connectionThread?.IsAlive ?? false;
		public bool IsConnected => _pipe?.IsConnected ?? false;

		string _server;
		string _pipeName;
		NamedPipeClientStream _pipe;

		Thread _connectionThread;
		Thread _writeThread;
		bool _cancelThreads;
		StreamWriter _streamWriter;

		public NamedPipeClient(string pipeName, string server = ".")
		{
			_pipeName = pipeName;
			_server = server;
		}

		public void Disconnect()
		{
			_cancelThreads = true;
			_writeThread?.Join();
			_pipe?.Close();
			_pipe?.Dispose();
			_pipe = null;
		}

		public void Dispose()
		{
			Disconnect();
		}

		public bool Connect(int timeout = Timeout.Infinite)
		{
			if (IsConnected)
				throw new InvalidOperationException("Already connected.");

			_cancelThreads = false;
			_pipe = new NamedPipeClientStream(_server, _pipeName, PipeDirection.Out, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

			try
			{
				_pipe.Connect(timeout); // it would normally throw UnauthorizedAccessException but Mono doesn't do that :(
			}
			catch { }

			if (!_pipe.IsConnected)
				return false;

			_streamWriter = new StreamWriter(_pipe)
			{
				AutoFlush = true
			};
			Connected?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public void ConnectAsync()
		{
			if (IsConnected)
				throw new InvalidOperationException("Already connected.");
			if (IsConnecting)
				throw new InvalidOperationException("Already connecting.");

			_cancelThreads = false;
			_connectionThread = new Thread(() =>
			{
				while (!_cancelThreads && !IsConnected)
				{
					if (Connect())
						break;
					Thread.Sleep(250);
				}
			});
			_connectionThread.Start();
		}

		public void SendMessage(string msg)
		{
			WriteAsync(msg);
		}

		void WriteAsync(string msg)
		{
			var threadState = _writeThread?.ThreadState ?? ThreadState.Unstarted;
			if (threadState == ThreadState.Running && !_writeThread.Join(5))
				return;

			_writeThread = new Thread(() =>
			{
				try
				{
					_streamWriter.Write(msg);
					_pipe.WaitForPipeDrain();
				}
				catch (IOException)
				{
					_pipe.Dispose();
					_pipe = null;
					Disconnected?.Invoke(this, EventArgs.Empty);
					ConnectAsync();
				}
			});
			_writeThread.Start();
		}
	}
}
