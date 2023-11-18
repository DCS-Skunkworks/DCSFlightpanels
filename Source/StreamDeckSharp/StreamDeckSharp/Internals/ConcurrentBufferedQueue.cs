using System;
using System.Collections.Generic;
using System.Threading;

namespace StreamDeckSharp.Internals
{
    internal sealed class ConcurrentBufferedQueue<TKey, TValue> : IDisposable
    {
        private readonly object _sync = new();

        private readonly Dictionary<TKey, TValue> _valueBuffer = new();
        private readonly Queue<TKey> _queue = new();

        private volatile bool _isAddingCompleted;
        private volatile bool _disposed;

        public int Count => _queue.Count;

        public bool IsAddingCompleted
        {
            get
            {
                ThrowIfDisposed();
                return _isAddingCompleted;
            }
        }

        public bool IsCompleted
        {
            get
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    return _isAddingCompleted && Count == 0;
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                if (_isAddingCompleted)
                {
                    throw new InvalidOperationException("Adding was already marked as completed.");
                }

                try
                {
                    _valueBuffer[key] = value;

                    if (!_queue.Contains(key))
                    {
                        _queue.Enqueue(key);
                    }
                }
                finally
                {
                    Monitor.PulseAll(_sync);
                }
            }
        }

        public (bool Success, TKey Key, TValue Value) Take()
        {
            lock (_sync)
            {
                while (_queue.Count < 1)
                {
                    ThrowIfDisposed();

                    if (_isAddingCompleted)
                    {
                        return (false, default, default);
                    }

                    Monitor.Wait(_sync);
                }

                ThrowIfDisposed();

                var key = _queue.Dequeue();
                var value = _valueBuffer[key];
                _valueBuffer.Remove(key);

                return (true, key, value);
            }
        }

        public void CompleteAdding()
        {
            lock (_sync)
            {
                if (_isAddingCompleted)
                {
                    return;
                }

                _isAddingCompleted = true;
                Monitor.PulseAll(_sync);
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                if (!_isAddingCompleted)
                {
                    CompleteAdding();
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ConcurrentBufferedQueue<TKey, TValue>));
            }
        }
    }
}
