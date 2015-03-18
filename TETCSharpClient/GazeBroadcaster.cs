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
        private readonly SynchronizedList<IGazeListener> gazeListeners;
        private readonly SingleFrameBlockingQueue<GazeData> blockingGazeQueue;
        private bool isRunning;
        private Thread workerThread;

        public GazeBroadcaster(SingleFrameBlockingQueue<GazeData> queue, SynchronizedList<IGazeListener> gazeListeners)
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

                blockingGazeQueue.Start();
                workerThread.Start();
            }
        }

        public void Stop()
        {
            lock (this)
            {
                isRunning = false;
                blockingGazeQueue.Stop();
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
                                ThreadPool.QueueUserWorkItem(new WaitCallback(HandleOnGazeUpdate), new Object[] { listener, gaze });
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

        internal static void HandleOnGazeUpdate(Object stateInfo)
        {
            IGazeListener listener = null;
            try
            {
                Object[] objs = (Object[])stateInfo;
                listener = (IGazeListener)objs[0];
                GazeData gaze = (GazeData)objs[1];
                listener.OnGazeUpdate(gaze);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while calling IGazeListener.OnGazeUpdate() on listener " + listener + ": " + e.Message);
            }
        }
    }
}
