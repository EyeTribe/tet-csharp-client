using System;
using System.Diagnostics;
using System.Threading;

namespace TETCSharpClient
{
    /// <summary>
    /// Class responsible for sending 'heartbeats' to the underlying TET C# Client Tracker notifying that the client is alive. 
    /// The Tracker Server defines the desired length of a heartbeat and is in this implementation automatically acquired through the Tracker API.
    /// </summary>
    internal class Heartbeat
    {
        private readonly GazeApiManager api;
        private bool isRunning;
        private Thread workerThread;

        public Heartbeat(GazeApiManager api)
        {
            this.api = api;
        }

        public void Start()
        {
            lock (this)
            {
                isRunning = true;
                ThreadStart ts = Work;
                workerThread = new Thread(ts);
                workerThread.Start();
            }
        }

        public void Stop()
        {
            lock (this)
            {
                isRunning = false;
            }
        }

        private void Work()
        {
            try
            {
                while (isRunning)
                {
                    if (api != null)
                        api.RequestHeartbeat();

                    Thread.Sleep(GazeManager.Instance.HeartbeatMillis);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while running Heartbeat: e" + e.Message);
            }
            finally
            {
                Debug.WriteLine("Heartbeat closing down");
            }
        }
    }

}

