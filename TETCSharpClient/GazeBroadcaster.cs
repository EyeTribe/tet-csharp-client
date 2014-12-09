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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TETCSharpClient.Data;

namespace TETCSharpClient
{
    /// <summary>
    /// Threaded broadcaster responsible for distributing GazeData update to all attached listeners.
    /// </summary>
    internal class GazeBroadcaster
    {
        private readonly List<IGazeListener> gazeListeners;
        private readonly SingleFrameBlockingQueue<GazeData> blockingGazeQueue;
        private bool isRunning;
        private Thread workerThread;

        public GazeBroadcaster(SingleFrameBlockingQueue<GazeData> queue, List<IGazeListener> gazeListeners)
        {
            this.gazeListeners = gazeListeners;
            this.blockingGazeQueue = queue;
        }

        public void Start()
        {
            lock (this)
            {
                isRunning = true;
                var ts = new ThreadStart(Work);
                workerThread = new Thread(ts);
                workerThread.Name = "GazeCallback";
                workerThread.Start();
            }
        }

        public void Stop()
        {
            lock (this)
            {
                isRunning = false;
                lock (blockingGazeQueue)
                {
                    Monitor.PulseAll(blockingGazeQueue);
                }
            }
        }

        private void Work()
        {
            try
            {
                //while thread not killed
                while (isRunning)
                {
                    GazeData gaze = blockingGazeQueue.Take();

                    if (null != gaze)
                    {
                        lock (gazeListeners)
                        {
                            foreach (IGazeListener listener in gazeListeners)
                            {
                                try
                                {
                                     listener.OnGazeUpdate(gaze);
                                }
                                catch (Exception e)
                                {
                                    Console.Write("Exception while calling IGazeListener.onGazeUpdate() " +
                                            "on listener " + listener + ": " + e.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while running broadcaster: " + e.Message);
            }
            finally
            {
                Debug.WriteLine("Broadcaster closing down");
            }
        }
    }

    internal class SingleFrameBlockingQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private bool isStopped;
        public SingleFrameBlockingQueue() { }

        public int Count { get { lock (queue) { return null != queue ? queue.Count : 0; } } }

        public void Put(T item)
        {
            lock (queue)
            {
                if (isStopped)
                    return;
                while (queue.Count > 0)
                    queue.Dequeue();
                queue.Enqueue(item);
                if (queue.Count == 1)
                    Monitor.PulseAll(queue);
            }
        }

        public T Take()
        {
            lock (queue)
            {
                if (isStopped)
                    return default(T);
                if (queue.Count == 0)
                    Monitor.Wait(queue);
                if (isStopped)
                    return default(T);
                else
                    return queue.Dequeue();
            }
        }

        public void Stop()
        {
            lock (queue)
            {
                queue.Clear();
                isStopped = true;
                Monitor.PulseAll(queue);
            }
        }

        public void Start()
        {
            lock (queue)
            {
                isStopped = false;
            }
        }
    }
}
