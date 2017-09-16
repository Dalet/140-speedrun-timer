using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace SpeedrunTimerMod.IPC
{
	class NamedPipeClient : IDisposable
	{
		public event EventHandler Connected;
		public event EventHandler Disconnected;

		public bool AutoReconnect { get; set; }
		public bool AutoCheckConnection { get; set; }
		public string PingMessage { get; set; } = "\n";

		public bool IsConnecting => !IsConnected && (_connectionThread?.IsAlive ?? false);
		public bool IsConnected => _pipe?.IsConnected ?? false;

		string _server;
		string _pipeName;
		NamedPipeClientStream _pipe;
		StreamWriter _streamWriter;

		Queue _messageQueue;

		Thread _connectionThread;
		Thread _writeThread;

		bool _cancellationRequested;
		ManualResetEvent _newMessagesEvent;
		ManualResetEvent _cancelEvent;

		public NamedPipeClient(string pipeName, string server = ".")
		{
			_pipeName = pipeName;
			_server = server;
			_messageQueue = new Queue();
			_newMessagesEvent = new ManualResetEvent(false);
			_cancelEvent = new ManualResetEvent(false);
		}

		public void Dispose()
		{
			Disconnect();
			_newMessagesEvent.Close();
			_cancelEvent.Close();
		}

		public void Disconnect()
		{
			StopThreads();
			ClearMessageQueue();

			if (_pipe == null)
				return;

			_pipe?.Close();
			_pipe?.Dispose();
			_pipe = null;
			Disconnected?.Invoke(this, EventArgs.Empty);
		}

		void OnUnexpectedDisconnection()
		{
			Disconnect();
			if (AutoReconnect)
				ConnectAsync();
		}

		public void ResetThreadCancellation()
		{
			_cancellationRequested = false;
			_cancelEvent.Reset();
		}

		public void StopThreads()
		{
			_cancellationRequested = true;
			_cancelEvent.Set();

			var currentThreadId = Thread.CurrentThread.ManagedThreadId;
			if (currentThreadId != _connectionThread?.ManagedThreadId)
				_connectionThread?.Join();
			if (currentThreadId != _writeThread?.ManagedThreadId)
				_writeThread?.Join();
		}

		public void ConnectAsync()
		{
			if (IsConnected || IsConnecting)
				return;

			ResetThreadCancellation();
			_connectionThread = new Thread(ConnectionThread);
			_connectionThread.Start();
		}

		void ConnectionThread()
		{
			try
			{
				while (!_cancellationRequested && !IsConnected)
				{
					if (Connect(10))
						break;
					_cancelEvent.WaitOne(250);
				}

				// keep checking the connection
				while (!_cancellationRequested && AutoCheckConnection && IsConnected)
				{
					_cancelEvent.WaitOne(150);
					CheckConnection();
				}
			}
			catch (Exception e)
			{
				Debug.Log("NamedPipeClient: Uncaught exception in connection thread:\n" + e.ToString());
				throw;
			}
		}

		public void CheckConnection()
		{
			if (!IsConnected)
				return;

			AsyncWrite(PingMessage);
		}

		bool Connect(int timeout = Timeout.Infinite)
		{
			if (IsConnected)
				throw new InvalidOperationException("Already connected.");

			_pipe = new NamedPipeClientStream(_server, _pipeName, PipeDirection.Out);

			try
			{
				_pipe.Connect(timeout); // it would normally throw UnauthorizedAccessException but Mono doesn't do that :(
			}
			catch { }

			if (!_pipe.IsConnected)
				return false;

			// prepare write thread
			_streamWriter = new StreamWriter(_pipe)
			{
				AutoFlush = true
			};

			_writeThread = new Thread(WriteThread);
			_writeThread.Start();

			Connected?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public void WaitAsyncWrite(string msg, int millisecondsTimeout = Timeout.Infinite)
		{
			var msgWrittenEvent = millisecondsTimeout != 0
				? new ManualResetEvent(false)
				: null;
			var pair = new KeyValuePair<ManualResetEvent, string>(msgWrittenEvent, msg);

			lock (_messageQueue.SyncRoot)
			{
				_messageQueue.Enqueue(pair);
				_newMessagesEvent.Set();
			}

			if (msgWrittenEvent != null)
			{
				WaitHandle.WaitAny(new WaitHandle[]
				{
					msgWrittenEvent,
					_cancelEvent
				}, millisecondsTimeout);
				msgWrittenEvent.Close();
			}
		}

		public void AsyncWrite(string msg)
		{
			WaitAsyncWrite(msg, 0);
		}

		void WriteThread()
		{
			try
			{
				var events = new WaitHandle[]
				{
					_newMessagesEvent,
					_cancelEvent
				};

				while (!_cancellationRequested && IsConnected)
				{
					WaitHandle.WaitAny(events);
					WriteQueue();
				}
			}
			catch (Exception e)
			{
				Debug.Log("NamedPipeClient: Uncaught exception in write thread:\n" + e.ToString());
				OnUnexpectedDisconnection();
				throw;
			}
		}

		bool WriteQueue()
		{
			var sb = new StringBuilder();
			IList<ManualResetEvent> messageWrittenEvents;
			lock (_messageQueue.SyncRoot)
			{
				_newMessagesEvent.Reset();
				messageWrittenEvents = new List<ManualResetEvent>(_messageQueue.Count);
				while (_messageQueue.Count > 0)
				{
					var pair = (KeyValuePair<ManualResetEvent, string>)_messageQueue.Dequeue();
					messageWrittenEvents.Add(pair.Key);
					sb.Append(pair.Value);
				}
			}

			var success = TryWrite(sb.ToString());

			foreach (var messageWrittenEvent in messageWrittenEvents)
			{
				if (messageWrittenEvent != null && !messageWrittenEvent.SafeWaitHandle.IsClosed)
					messageWrittenEvent.Set();
			}

			return success;
		}

		void ClearMessageQueue()
		{
			lock (_messageQueue.SyncRoot)
			{
				while (_messageQueue.Count > 0)
				{
					var pair = (KeyValuePair<ManualResetEvent, string>)_messageQueue.Dequeue();
					var resetEvent = pair.Key;
					if (resetEvent != null && !resetEvent.SafeWaitHandle.IsClosed)
						resetEvent.Set();
				}
			}
		}

		bool TryWrite(string message)
		{
			var success = false;
			try
			{
				_streamWriter.Write(message);
				_pipe.WaitForPipeDrain();
				success = true;
			}
			catch (IOException e)
			{
				var str = !_pipe.IsConnected ? "Disconnected!" : "";
				Debug.Log($"NamedPipeClient.TryWrite: Write failed! {str} IOException:\n" + e.ToString());
				if (!_cancellationRequested)
					OnUnexpectedDisconnection();
			}
			catch (Exception)
			{
				Debug.Log($"NamedPipeClient.TryWrite: Write failed! Unexpected exception");
				throw;
			}

			return success;
		}
	}
}
