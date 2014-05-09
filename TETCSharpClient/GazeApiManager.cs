using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using TETCSharpClient.Request;
using System.IO;
using System.Text;
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
        private IGazeApiConnectionListener connectionListener;

        private readonly Object initializationLock = new Object();

        #endregion

        #region Constructor

        public GazeApiManager(IGazeApiReponseListener responseListener)
            : this(responseListener, null) { }

        public GazeApiManager(IGazeApiReponseListener responseListener, IGazeApiConnectionListener connectionListener)
        {
            this.responseListener = responseListener;
            this.connectionListener = connectionListener;
            requestQueue = new Queue<String>();
        }

        #endregion

        #region Public methods

        public bool Connect(string host, int port)
        {
            lock (initializationLock)
            {
                Close();

                try
                {
                    outEvent = new WaitHandleWrap();
                    socket = new TcpClient(host, port);

                    //notify connection change
                    if (null != connectionListener)
                        connectionListener.OnGazeApiConnectionStateChanged(socket.Connected);

                    incomingStreamHandler = new IncomingStreamHandler(socket, responseListener, connectionListener, this);
                    incomingStreamHandler.Start();

                    outgoingStreamHandler = new OutgoingStreamHandler(socket, requestQueue, outEvent, connectionListener, this);
                    outgoingStreamHandler.Start();
                }
                catch (SocketException se)
                {
                    Debug.WriteLine("Unable to open socket. Is Tracker Server running? Exception: " + se.Message);

                    //notify connection change
                    if (null != connectionListener)
                        connectionListener.OnGazeApiConnectionStateChanged(false);

                    Close();
                    return false;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception while establishing socket connection. Is Tracker Server running? Exception: " + e.Message);
                    Close();
                    return false;
                }
                return true;
            }
        }

        public void Close()
        {
            lock (initializationLock)
            {
                try
                {
                    if (null != requestQueue)
                        lock (((ICollection)requestQueue).SyncRoot)
                        {
                            requestQueue.Clear();
                        }

                    if (null != incomingStreamHandler)
                    {
                        incomingStreamHandler.Stop();
                        incomingStreamHandler = null;
                    }

                    if (null != outgoingStreamHandler)
                    {
                        outgoingStreamHandler.Stop();
                        outgoingStreamHandler = null;
                    }

                    if (null != socket)
                    {
                        socket.Close();
                        socket = null;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error closing socket: " + e.Message);
                }
            }
        }

        public bool IsConnected()
        {
            if (null != socket)
                return socket.Connected;

            return false;
        }

        protected void Request(string request)
        {
            if (IsConnected())
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

            gr.Values.Version = (int)Convert.ChangeType(version, version.GetTypeCode());
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
            gr.Values.ScreenPhysicalWidth = screenPsyW;
            gr.Values.ScreenPhysicalHeight = screenPsyH;

            Request(JsonConvert.SerializeObject(gr));
        }

        #endregion
    }

    internal class IncomingStreamHandler
    {
        private bool isRunning;
        private Thread workerThread;
        private TcpClient socket;
        private StreamReader reader;
        private IGazeApiReponseListener responseListener;
        private IGazeApiConnectionListener connectionListener;
        private GazeApiManager networkLayer;

        public IncomingStreamHandler(TcpClient _socket, IGazeApiReponseListener _responseListener, IGazeApiConnectionListener _connectionListener, GazeApiManager _networkLayer)
        {
            this.socket = _socket;
            this.responseListener = _responseListener;
            this.connectionListener = _connectionListener;
            this.networkLayer = _networkLayer;
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
                if (null != reader)
                    reader.Close();

                isRunning = false;
            }
        }

        private bool IsSocketConnected()
        {
            return !((null != socket && socket.Client.Poll(1000, SelectMode.SelectRead) && (socket.Client.Available == 0)) || !socket.Client.Connected);
        }

        private void Work()
        {
            try
            {
                reader = new StreamReader(socket.GetStream(), Encoding.ASCII);

                while (isRunning)
                {
                    try
                    {
                        while (!reader.EndOfStream)
                        {
                            string response = reader.ReadLine();

                            if (null != responseListener && !string.IsNullOrEmpty(response))
                                responseListener.OnGazeApiResponse(response);
                        }
                    }
                    catch (IOException ioe)
                    {
                        //Are we closing down or has reading from socket failed?
                        if (isRunning && !IsSocketConnected())
                        {
                            //notify connection listener if any
                            if (null != connectionListener)
                                connectionListener.OnGazeApiConnectionStateChanged(false);

                            //server must be disconnected, shut down network layer
                            networkLayer.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception while reading from socket: " + e.Message);
                    }

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while establishing incoming socket connection: " + e.Message);
            }
            finally
            {
                Debug.WriteLine("IncommingStreamHandler closing down");
            }
        }
    }

    internal class OutgoingStreamHandler
    {
        private readonly int NUM_WRITE_ATTEMPTS_BEFORE_FAIL = 3;
        private int numWriteAttempt;

        private Thread workerThread;
        private TcpClient socket;
        private StreamWriter writer;
        private Queue<String> queue;
        private WaitHandleWrap events;
        private IGazeApiConnectionListener connectionListener;
        private GazeApiManager networkLayer;

        public OutgoingStreamHandler(TcpClient _socket, Queue<String> _queue, WaitHandleWrap _events, IGazeApiConnectionListener _connectionListener, GazeApiManager _networkLayer)
        {
            this.socket = _socket;
            this.queue = _queue;
            this.events = _events;
            this.connectionListener = _connectionListener;
            this.networkLayer = _networkLayer;
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
                if (null != writer)
                    writer.Close();

                events.GetKillHandle().Set();
                events.GetUpdateHandle().Set();
            }
        }

        private void Work()
        {
            try
            {
                string request = string.Empty;
                writer = new StreamWriter(socket.GetStream(), Encoding.ASCII);

                //while waiting for queue to populate and thread not killed
                while (!events.GetKillHandle().WaitOne(0, false))
                {
                    try
                    {
                        events.GetUpdateHandle().WaitOne();

                        //handle all pending request before going to sleep
                        while (queue.Count > 0)
                        {
                            lock (((ICollection)queue).SyncRoot)
                            {
                                request = queue.Dequeue();
                            }

                            writer.WriteLine(request);
                            writer.Flush();

                            if (numWriteAttempt > 0)
                                numWriteAttempt = 0;
                        }
                    }
                    catch (IOException ioe)
                    {
                        //Has writing to socket failed and may server be disconnected?
                        if (numWriteAttempt++ >= NUM_WRITE_ATTEMPTS_BEFORE_FAIL)
                        {
                            //notify connection listener if any
                            if (null != connectionListener)
                                connectionListener.OnGazeApiConnectionStateChanged(false);

                            //server must be disconnected, shut down network layer
                            networkLayer.Close();
                        }
                        else
                        {
                            //else retry request asap
                            queue.Enqueue(request);
                            //Signal Event that queue is populated
                            events.GetUpdateHandle().Set();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception while writing to socket: " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while establishing outgoing socket connection: " + e.Message);
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
    internal interface IGazeApiReponseListener
    {
        void OnGazeApiResponse(String response);
    }

    //<summary>
    // Callback interface responsible for handling connection state notifications from the GazeApiManager
    // </summary>
    internal interface IGazeApiConnectionListener
    {
        void OnGazeApiConnectionStateChanged(bool isConnected);
    }
}
