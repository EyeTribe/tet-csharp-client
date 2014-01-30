using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using TETCSharpClient.Request;
using System.IO;
using System.Diagnostics;

namespace TETCSharpClient
{
    //<summary>
    // This class manages communication with the underlying Tracker Server using
    // the Tracker API over TCP Sockets.
    // </summary>
    internal class GazeApiManager
    {
        #region Constants

        internal const string DEFAULT_SERVER_HOST = "localhost";
        internal const int DEFAULT_SERVER_PORT = 6555;

        #endregion

        #region Variables

        private TcpClient socket;
        private IncomingStreamHandler incomingStreamHandler;
        private OutgoingStreamHandler outgoingStreamHandler;
        private WaitHandleWrap outEvent;
        private Queue<String> requestQueue;
        private IGazeApiReponseListener responseListener;

        #endregion

        #region Constructor

        public GazeApiManager(IGazeApiReponseListener respListener)
        {
            responseListener = respListener;
            requestQueue = new Queue<String>();
        }

        #endregion

        #region Public methods

        public bool Connect(string host, int port)
        {
            Close();

            try
            {
                outEvent = new WaitHandleWrap();
                socket = new TcpClient(host, port);

                incomingStreamHandler = new IncomingStreamHandler(socket, responseListener);
                incomingStreamHandler.Start();

                outgoingStreamHandler = new OutgoingStreamHandler(socket, requestQueue, outEvent);
                outgoingStreamHandler.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error opening socket. Is Tracker Server running? " + e.Message);
                Close();
                return false;
            }
            return true;
        }
	
	    public void Close()
	    {
		    try 
		    {
			    if(null != incomingStreamHandler)
				    incomingStreamHandler.Stop();
			
			    if(null != outgoingStreamHandler)
                    outgoingStreamHandler.Stop();
			
			    if(null != socket)
				    socket.Close();

                if (null != requestQueue)
                    requestQueue.Clear();
		    }
		    catch (Exception e) 
		    {
                Debug.WriteLine("Error closing socket: " + e.Message);
		    }
	    }
	
	    public bool IsConnected()
	    {
		    if(null != socket)
			    return socket.Connected;
				
		    return false;
	    }

	    protected void Request(string request)
	    {
            lock (((ICollection)requestQueue).SyncRoot)
            {
                requestQueue.Enqueue(request);
                //Signal Event that queue is populated
                outEvent.GetUpdateHandle().Set();
            }
	    }

        public void RequestTracker(GazeManager.ClientMode mode, GazeManager.ApiVersion version)
        {
            TrackerSetRequest gr = new TrackerSetRequest();

            gr.Values.Version = (int)version.GetTypeCode();
            gr.Values.Push = mode == GazeManager.ClientMode.Push;

            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestAllStates()
        {
            TrackerGetRequest gr = new TrackerGetRequest();

            gr.Values = new[]
			{
				Protocol.TRACKER_HEARTBEATINTERVAL,
				Protocol.TRACKER_ISCALIBRATED,
				Protocol.TRACKER_ISCALIBRATING,
                Protocol.TRACKER_TRACKERSTATE,
				Protocol.TRACKER_SCREEN_INDEX,
                Protocol.TRACKER_SCREEN_RESOLUTION_WIDTH,
                Protocol.TRACKER_SCREEN_RESOLUTION_HEIGHT,
                Protocol.TRACKER_SCREEN_PHYSICAL_WIDTH,
                Protocol.TRACKER_SCREEN_PHYSICAL_HEIGHT,
                Protocol.TRACKER_CALIBRATIONRESULT,
                Protocol.TRACKER_FRAMERATE,
				Protocol.TRACKER_VERSION,
				Protocol.TRACKER_MODE_PUSH
			};

            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestCalibrationStates()
        {
            TrackerGetRequest gr = new TrackerGetRequest();

            gr.Values = new[]
			{
				Protocol.TRACKER_ISCALIBRATED,
				Protocol.TRACKER_ISCALIBRATING,
                Protocol.TRACKER_CALIBRATIONRESULT
			};

            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestScreenStates()
        {
            TrackerGetRequest gr = new TrackerGetRequest();

            gr.Values = new[]
			{
				Protocol.TRACKER_SCREEN_INDEX,
                Protocol.TRACKER_SCREEN_RESOLUTION_WIDTH,
                Protocol.TRACKER_SCREEN_RESOLUTION_HEIGHT,
                Protocol.TRACKER_SCREEN_PHYSICAL_WIDTH,
                Protocol.TRACKER_SCREEN_PHYSICAL_HEIGHT
			};

            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestTrackerState()
        {
            TrackerGetRequest gr = new TrackerGetRequest();

            gr.Values = new[]
			{
				Protocol.TRACKER_TRACKERSTATE,
                Protocol.TRACKER_FRAMERATE
			};

            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestHeartbeat()
        {
            RequestBase gr = new RequestBase();
            gr.Category = Protocol.CATEGORY_HEARTBEAT;
            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestCalibrationStart(int pointcount)
        {
            Request(JsonConvert.SerializeObject(new CalibrationStartRequest(pointcount)));
        }

        public void RequestCalibrationPointStart(int x, int y)
        {
            Request(JsonConvert.SerializeObject(new CalibrationPointStartRequest(x, y)));
        }

        public void RequestCalibrationPointEnd()
        {
            RequestBase gr = new RequestBase();
            gr.Category = Protocol.CATEGORY_CALIBRATION;
            gr.Request = Protocol.CALIBRATION_REQUEST_POINTEND;

            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestCalibrationAbort()
        {
            RequestBase gr = new RequestBase();
            gr.Category = Protocol.CATEGORY_CALIBRATION;
            gr.Request = Protocol.CALIBRATION_REQUEST_ABORT;

            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestCalibrationClear()
        {
            RequestBase gr = new RequestBase();
            gr.Category = Protocol.CATEGORY_CALIBRATION;
            gr.Request = Protocol.CALIBRATION_REQUEST_CLEAR;

            Request(JsonConvert.SerializeObject(gr));
        }

        public void RequestScreenSwitch(int screenIndex, int screenResW, int screenResH, float screenPsyW, float screenPsyH)
        {
            TrackerSetRequest gr = new TrackerSetRequest();

            gr.Values.ScreenIndex = screenIndex;
            gr.Values.ScreenResolutionWidth = screenResW;
            gr.Values.ScreenResolutionHeight = screenResH;
            gr.Values.ScreenPhysicalWidth = screenResW;
            gr.Values.ScreenPhysicalHeight = screenResH;

            Request(JsonConvert.SerializeObject(gr));
        }

        #endregion
    }

    internal class IncomingStreamHandler
    {
        private bool isRunning;
        private Thread workerThread;
        private TcpClient socket;
        private IGazeApiReponseListener responseListener;

        public IncomingStreamHandler(TcpClient _socket, IGazeApiReponseListener respListener)
        {
            socket = _socket;
            responseListener = respListener;
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
                StreamReader reader  = new StreamReader(socket.GetStream());

                while (isRunning)
                {
                    string response = reader.ReadLine();

                    if (null != responseListener)
                        responseListener.OnGazeApiResponse(response);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while running IncomingStreamHandler: " + e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                Debug.WriteLine("IncommingStreamHandler closing down");
            }           
        }
    }

    internal class OutgoingStreamHandler
    {
        private Thread workerThread;
        private TcpClient socket;
        private Queue<String> queue;
        private WaitHandleWrap events;

        public OutgoingStreamHandler(TcpClient socket, Queue<String> queue, WaitHandleWrap events)
        {
            this.socket = socket;
            this.queue = queue;
            this.events = events;
        }

        public void Start()
        {
            lock (this)
            {
                ThreadStart ts = Work;
                workerThread = new Thread(ts);
                workerThread.Start();
            }
        }

        public void Stop()
        {
            lock (this)
            {
                events.GetKillHandle().Close();
                events.GetUpdateHandle().Set();
                events.GetUpdateHandle().Close();
            }
        }

        private void Work()
        {
            try
            {
                string request = string.Empty;
                StreamWriter writer = new StreamWriter(socket.GetStream());

                //while waiting for queue to populate and thread not killed
                while (!events.GetKillHandle().WaitOne(0, false))
                {
                    events.GetUpdateHandle().WaitOne();

                    //handle all pending request before going to sleep
                    while(queue.Count > 0)
                    {
                        lock (((ICollection)queue).SyncRoot)
                        {
                            request = queue.Dequeue();
                        }

                        writer.WriteLine(request);
                        writer.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while running OutgoingStreamHandler: " + e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                Debug.WriteLine("OutgoingStreamHandler closing down");
            }
        }
    }

    //<summary>
    // Callback interface responsible for handling messaages returned from the GazeApiManager 
    // </summary>
    public interface IGazeApiReponseListener 
    {
        void OnGazeApiResponse(String response);
    }
}
