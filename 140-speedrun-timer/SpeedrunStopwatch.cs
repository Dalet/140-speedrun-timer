using System;
using System.Diagnostics;

namespace SpeedrunTimerMod
{
	class SpeedrunStopwatch
	{
        public TimeSpan RealTime => _realTime.Elapsed + _realTimeOffset;
        public TimeSpan GameTime => _gameTime.Elapsed + _gameTimeOffset;

        public bool IsStarted => _realTime.IsRunning;
		public bool IsPaused => _realTime.IsRunning && !_gameTime.IsRunning;
		public DateTime StartDate { get; private set; }

		Stopwatch _realTime;
		Stopwatch _gameTime;
        TimeSpan _realTimeOffset;
        TimeSpan _gameTimeOffset;

		public SpeedrunStopwatch()
        {
            _realTime = new Stopwatch();
            _gameTime = new Stopwatch();
            _realTimeOffset = _gameTimeOffset = TimeSpan.Zero;
        }

		public void Reset()
        {
            _realTime.Reset();
            _gameTime.Reset();
            _realTimeOffset = _gameTimeOffset = TimeSpan.Zero;
        }

        public void Start(int millisecondsOffset = 0)
        {
            if (IsStarted)
                return;

            _realTime.Start();
            _gameTime.Start();

			_realTimeOffset = _gameTimeOffset = TimeSpan.FromMilliseconds(millisecondsOffset * -1);
			StartDate = DateTime.UtcNow + _realTimeOffset;
        }

		public void Stop(int millisecondsOffset = 0)
		{
			if (!IsStarted)
				return;

			_gameTime.Stop();
			_realTime.Stop();

			var offset = TimeSpan.FromMilliseconds(millisecondsOffset);
			_realTimeOffset += offset;
			_gameTimeOffset += offset;
		}

		public void Pause(int millisecondsOffset = 0)
        {
            if (!IsStarted || IsPaused)
                return;

            _gameTime.Stop();
            _gameTimeOffset += TimeSpan.FromMilliseconds(millisecondsOffset);
        }

		public void Resume(int millisecondsOffset = 0)
        {
            if (!IsStarted || !IsPaused)
                return;

            _gameTime.Start();
            _gameTimeOffset -= TimeSpan.FromMilliseconds(millisecondsOffset);
        }
    }
}
