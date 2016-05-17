/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace EyeTribe.ClientSdk
{
    public class PriorityBlockingQueue<T> where T : IComparable<T>
    {
        private List<T> _List;
        private readonly object _Lock;
        private bool _Stopped;

        public PriorityBlockingQueue()
        {
            _List = new List<T>();
            _Lock = ((ICollection)_List).SyncRoot;
        }

        public int Count { get { lock (_Lock) { return null != _List ? _List.Count : 0; } } }

        public void Enqueue(T item)
        {
            lock (_Lock)
            {
                if (_Stopped)
                {
                    Monitor.PulseAll(_Lock);
                    return;
                }

                _List.Add(item);
                int ci = _List.Count - 1;
                while (ci > 0)
                {
                    int pi = (ci - 1) / 2;
                    if (_List[ci].CompareTo(_List[pi]) >= 0)
                        break;
                    T tmp = _List[ci]; _List[ci] = _List[pi]; _List[pi] = tmp;
                    ci = pi;
                }

                if (_List.Count >= 1)
                    Monitor.PulseAll(_Lock);
            }
        }

        public T Dequeue()
        {
            lock (_Lock)
            {
                while (_List.Count == 0)
                {
                    if (_Stopped)
                    {
                        Monitor.PulseAll(_Lock);
                        return default(T);
                    }
                    Monitor.Wait(_Lock);
                }

                int li = _List.Count - 1;
                T frontItem = _List[0];
                _List[0] = _List[li];
                _List.RemoveAt(li);

                --li;
                int pi = 0;
                while (true)
                {
                    int ci = pi * 2 + 1;
                    if (ci > li) break;
                    int rc = ci + 1;
                    if (rc <= li && _List[rc].CompareTo(_List[ci]) < 0)
                        ci = rc;
                    if (_List[pi].CompareTo(_List[ci]) <= 0) break;
                    T tmp = _List[pi]; _List[pi] = _List[ci]; _List[ci] = tmp;
                    pi = ci;
                }
                return frontItem;
            }
        }

        public List<T> GetList()
        {
            return _List;
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
                _List.Clear();
                _Stopped = true;
                Monitor.PulseAll(_Lock);
            }
        }
    }
}
