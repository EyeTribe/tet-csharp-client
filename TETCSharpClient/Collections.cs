using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TETCSharpClient
{
    /// <summary>
    /// Threadsafe blocking queue implementation for .NET 3.5 compliance.
    /// <p>
    /// The Collection will lock calls to Dequeue until objects are added.
    /// <p>
    /// The queue can optionally be set to have a max size. If max size is
    /// reached, the queue will lock until objects are dequeued.
    /// </summary>
    internal class BlockingQueue<T>
    {
        private readonly Queue<T> _Queue;
        private readonly object _Lock;
        private bool _Stopped;
        private int _MaxSize = -1;

        public BlockingQueue()
            : this(-1)
        {
            //unlimited size
        }

        public BlockingQueue(int maxSize)
        {
            _Queue = new Queue<T>();
            _Lock = ((ICollection)_Queue).SyncRoot;
            _MaxSize = maxSize;
        }

        public int Count { get { lock (_Lock) { return null != _Queue ? _Queue.Count : 0; } } }

        public void Enqueue(T item)
        {
            lock (_Lock)
            {
                while (_MaxSize > 0 && _Queue.Count >= _MaxSize)
                {
                    if (_Stopped)
                    {
                        Monitor.PulseAll(_Lock);
                        return;
                    }
                    Monitor.Wait(_Lock);
                }
                _Queue.Enqueue(item);
                if (_Queue.Count >= 1)
                    Monitor.PulseAll(_Lock);
            }
        }

        public T Dequeue()
        {
            lock (_Lock)
            {
                while (_Queue.Count == 0)
                {
                    if (_Stopped)
                    {
                        Monitor.PulseAll(_Lock);
                        return default(T);
                    }
                    Monitor.Wait(_Lock);
                }
                T item = _Queue.Dequeue();
                if (_Queue.Count >= 1)
                    Monitor.PulseAll(_Lock);
                return item;
            }
        }

        public void Start()
        {
            lock (_Lock)
            {
                _Stopped = false;
                Monitor.PulseAll(_Lock);
            }
        }

        public void Stop()
        {
            lock (_Lock)
            {
                _Queue.Clear();
                _Stopped = true;
                Monitor.PulseAll(_Lock);
            }
        }
    }

    /// <summary>
    /// Threadsafe single frame blocking queue implementation for .NET 3.5 compliance.
    /// <p>
    /// The Collection holds a single object at all times and discards old objects as new are added.
    /// </summary>
    internal class SingleFrameBlockingQueue<T>
    {
        private readonly Queue<T> _Queue;
        private readonly object _Lock;
        private bool _Stopped;

        public SingleFrameBlockingQueue()
        {
            _Queue = new Queue<T>();
            _Lock = ((ICollection)_Queue).SyncRoot;
        }

        public int Count { get { lock (_Lock) { return null != _Queue ? _Queue.Count : 0; } } }

        public void Put(T item)
        {
            lock (_Lock)
            {
                if (_Stopped)
                    return;
                while (_Queue.Count > 0)
                    _Queue.Dequeue();
                _Queue.Enqueue(item);
                if (_Queue.Count == 1)
                    Monitor.PulseAll(_Lock);
            }
        }

        public T Take()
        {
            lock (_Lock)
            {
                if (_Stopped)
                    return default(T);
                while (_Queue.Count == 0)
                {
                    if (_Stopped)
                        return default(T);
                    Monitor.Wait(_Lock);
                }
                T item = _Queue.Dequeue();
                if (_Queue.Count >= 1)
                    Monitor.PulseAll(_Lock);
                return item;
            }
        }

        public void Stop()
        {
            lock (_Lock)
            {
                _Stopped = true;
                _Queue.Clear();
                Monitor.PulseAll(_Lock);
            }
        }

        public void Start()
        {
            lock (_Lock)
            {
                _Stopped = false;
                Monitor.PulseAll(_Lock);
            }
        }
    }


    /// <summary>
    /// Threadsafe List implementation for .NET 3.5 compliance
    /// </summary>
    internal class SynchronizedList<T> : IList<T>
    {
        private readonly List<T> _List;
        private readonly object _Lock;

        public SynchronizedList()
        {
            _List = new List<T>();
            _Lock = ((ICollection)_List).SyncRoot;
        }

        public int IndexOf(T item)
        {
            lock (_Lock)
                return _List.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            lock (_Lock)
                _List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            lock (_Lock)
                _List.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                lock (_Lock)
                    return _List[index];
            }
            set
            {
                lock (_Lock)
                    _List[index] = value;
            }
        }

        public void Add(T item)
        {
            lock (_Lock)
                _List.Add(item);
        }

        public void Clear()
        {
            lock (_Lock)
                _List.Clear();
        }

        public bool Contains(T item)
        {
            lock (_Lock)
                return _List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_Lock)
                _List.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                lock (_Lock)
                    return _List.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            lock (_Lock)
                return _List.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SafeEnumerator<T>(_List.GetEnumerator(), _Lock);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Threadsafe Enumerator implementation for .NET 3.5 compliance
    /// </summary>
    public class SafeEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> _Inner;
        private readonly object _Lock;

        public SafeEnumerator(IEnumerator<T> inner, object @lock)
        {
            _Inner = inner;
            _Lock = @lock;
            Monitor.Enter(_Lock);
        }

        public void Dispose()
        {
            Monitor.Exit(_Lock);
        }

        public bool MoveNext()
        {
            return _Inner.MoveNext();
        }

        public void Reset()
        {
            _Inner.Reset();
        }

        public T Current
        {
            get { return _Inner.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
