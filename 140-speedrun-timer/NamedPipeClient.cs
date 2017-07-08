using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace SpeedrunTimerMod
{
	class NamedPipeClient : IDisposable
	{
		public event EventHandler Connected;
		public event EventHandler Disconnected;

		public bool IsConnecting => _connectionThread?.IsAlive ?? false;
		public bool IsConnected => _pipe?.IsConnected ?? false;
		public bool AutoReconnect { get; set; }

		string _server;
		string _pipeName;
		NamedPipeClientStream _pipe;
		StreamWriter _streamWriter;

		Queue _messageQueue;

		Thread _connectionThread;
		Thread _writeThread;

		bool _connectionThreadCancelled;
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

		public void Disconnect()
		{
			CancelThreads();
			try
			{
				_streamWriter?.Dispose();
			} catch { }
			_pipe?.Close();
			_pipe?.Dispose();
			_pipe = null;
			Disconnected?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			Disconnect();
			_newMessagesEvent.Close();
			_cancelEvent.Close();
		}

		public void ResetThreadCancellation()
		{
			_connectionThreadCancelled = false;
			_cancelEvent.Reset();
		}

		public void CancelThreads()
		{
			_connectionThreadCancelled = true;
			_cancelEvent.Set();
			_connectionThread?.Join();
			_writeThread?.Join();
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

		public void ConnectAsync()
		{
			if (IsConnected || IsConnecting)
				return;

			CancelThreads();
			ResetThreadCancellation();
			_connectionThread = new Thread(() =>
			{
				while (!_connectionThreadCancelled && !IsConnected)
				{
					if (Connect(10))
						break;
					if (!_connectionThreadCancelled)
						Thread.Sleep(250);
				}
			});
			_connectionThread.Start();
		}

		public void WaitWrite(string msg, int millisecondsTimeout = Timeout.Infinite)
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

		public void Write(string msg)
		{
			WaitWrite(msg, 0);
		}

		void WriteThread()
		{
			var events = new WaitHandle[]
			{
				_newMessagesEvent,
				_cancelEvent
			};

			while (!_connectionThreadCancelled && IsConnected)
			{
				WaitHandle.WaitAny(events);
				WriteQueue();
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
				messageWrittenEvent?.Set();

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
					resetEvent.Set();
					resetEvent.Close();
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
			catch (IOException)
			{
				_pipe.Dispose();
				_pipe = null;
				if (!_connectionThreadCancelled && AutoReconnect)
					ConnectAsync();
				Disconnected?.Invoke(this, EventArgs.Empty);
			}
			return success;
		}
	}
}
