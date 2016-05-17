/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EyeTribe.ClientSdk.Request;
using EyeTribe.ClientSdk.Response;

namespace EyeTribe.ClientSdk
{
    //<summary>
    // This class manages communication with the underlying Tracker Server using
    // the Tracker API over TCP Sockets.
    // </summary>
    public class GazeApiManager
    {
        #region Constants

        internal const string DEFAULT_SERVER_HOST = "localhost";
        internal const int DEFAULT_SERVER_PORT = 6555;

        #endregion

        #region Variables

        private TcpClient _Socket;
        private IncomingStreamHandler _IncomingStreamHandler;
        private OutgoingStreamHandler _OutgoingStreamHandler;
        private IGazeApiReponseListener _ResponseListener;
        private IGazeApiConnectionListener _ConnectionListener;

        private readonly Object _InitializationLock = new Object();

        private int _RequestID;

        private PriorityBlockingQueue<IRequest> _RequestQueue;
        private ConcurrentDictionary<int, IRequest> _OngoingRequests;

        #endregion

        #region Constructor

        public GazeApiManager(IGazeApiReponseListener responseListener)
            : this(responseListener, null) { }

        public GazeApiManager(IGazeApiReponseListener responseListener, IGazeApiConnectionListener connectionListener)
        {
            this._ResponseListener = responseListener;
            this._ConnectionListener = connectionListener;
        }

        #endregion

        #region Public methods

        public bool Connect(string host, int port, long timeout)
        {
            lock (_InitializationLock)
            {
                if (IsConnected())
                    Close();

                try
                {
                    _Socket = new TcpClient();

                    _RequestQueue = new PriorityBlockingQueue<IRequest>();
                    _RequestQueue.Start();
                    _OngoingRequests = new ConcurrentDictionary<int, IRequest>();

                    IAsyncResult result = _Socket.BeginConnect(host, port, null, null);
                    bool connected = result.AsyncWaitHandle.WaitOne((int)timeout);

                    if (connected)
                    {
                        _Socket.EndConnect(result);
                        _Socket.ReceiveTimeout = (int)timeout;

                        //notify connection change
                        if (null != _ConnectionListener)
                            _ConnectionListener.OnGazeApiConnectionStateChanged(_Socket.Connected);

                        _IncomingStreamHandler = new IncomingStreamHandler(_Socket, _ResponseListener, _ConnectionListener, _OngoingRequests, this);
                        _IncomingStreamHandler.Start();

                        _OutgoingStreamHandler = new OutgoingStreamHandler(_Socket, _RequestQueue, _OngoingRequests, _ConnectionListener, this);
                        _OutgoingStreamHandler.Start();

                        return true;
                    }
                }
                catch (SocketException se)
                {
                    Debug.WriteLine("Unable to open socket. Is Tracker Server running? Exception: " + se.Message);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception while establishing socket connection. Is Tracker Server running? Exception: " + e.Message);
                }

                Close();

                return false;
            }
        }

        public void Close()
        {
            lock (_InitializationLock)
            {
                try
                {
                    if (null != _Socket)
                    {
                        _Socket.Close();
                        _Socket = null;
                    }

                    if (null != _IncomingStreamHandler)
                    {
                        _IncomingStreamHandler.Stop();
                        _IncomingStreamHandler = null;
                    }

                    if (null != _OutgoingStreamHandler)
                    {
                        _OutgoingStreamHandler.Stop();
                        _OutgoingStreamHandler = null;
                    }

                    //notify connection change
                    if (null != _ConnectionListener)
                        _ConnectionListener.OnGazeApiConnectionStateChanged(false);

                    //We cancel queued requests
                    if (null != _RequestQueue) 
                    {
                        CancelAllRequests();
                        _RequestQueue.Stop();
                    }
                    _RequestQueue = null;

                    //We make sure we cancel currently ongoing requests
                    if (null != _OngoingRequests)
                    {
                        IEnumerator<KeyValuePair<int, IRequest>> reqs = _OngoingRequests.GetEnumerator();

                        while(reqs.MoveNext())
                        {
                            reqs.Current.Value.Cancel();
                        }
                        _OngoingRequests.Clear();
                    }
                    _OngoingRequests = null;

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error closing socket: " + e.Message);
                }
            }
        }

        public bool IsConnected()
        {
            if (null != _Socket)
                return _Socket.Connected;

            return false;
        }

        protected void Request(IRequest request)
        {
            lock (_RequestQueue)
            {
                request.Id = ++_RequestID;

                _RequestQueue.Enqueue(request);
            }
        }

        public void CancelAllRequests()
        {
            lock (_RequestQueue)
            {
                foreach(RequestBase<ResponseBase> r in _RequestQueue.GetList())
                {
                    r.Cancel();
                }
            }
        }

        public void RequestTracker(GazeManager.ApiVersion version)
        {
            TrackerSetRequest tsr = new TrackerSetRequest();

            tsr.Values.Version = (int)Convert.ChangeType(version, version.GetTypeCode());

            Request(tsr);
        }

        public void RequestAllStates()
        {
            TrackerGetRequest tgr = new TrackerGetRequest();

            tgr.Values = new[]
			{
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
				Protocol.TRACKER_VERSION
			};

            Request(tgr);
        }

        public void RequestCalibrationStates()
        {
            TrackerGetRequest tgr = new TrackerGetRequest();

            tgr.Values = new[]
			{
				Protocol.TRACKER_ISCALIBRATED,
				Protocol.TRACKER_ISCALIBRATING,
                Protocol.TRACKER_CALIBRATIONRESULT
			};

            Request(tgr);
        }

        public void RequestScreenStates()
        {
            TrackerGetRequest tgr = new TrackerGetRequest();

            tgr.Values = new[]
			{
				Protocol.TRACKER_SCREEN_INDEX,
                Protocol.TRACKER_SCREEN_RESOLUTION_WIDTH,
                Protocol.TRACKER_SCREEN_RESOLUTION_HEIGHT,
                Protocol.TRACKER_SCREEN_PHYSICAL_WIDTH,
                Protocol.TRACKER_SCREEN_PHYSICAL_HEIGHT
			};

            Request(tgr);
        }

        public void RequestTrackerState()
        {
            TrackerGetRequest tgr = new TrackerGetRequest();

            tgr.Values = new[]
			{
				Protocol.TRACKER_TRACKERSTATE,
                Protocol.TRACKER_FRAMERATE
			};

            Request(tgr);
        }

        public Object RequestCalibrationStart(int pointcount)
        {
            CalibrationStartRequest csr = new CalibrationStartRequest(pointcount);

            csr.AsyncLock = new Object();

            Request(csr);

            return csr.AsyncLock;
        }

        public void RequestCalibrationPointStart(int x, int y)
        {
            CalibrationPointStartRequest cpsr = new CalibrationPointStartRequest(x,y);

            Request(cpsr);
        }

        public void RequestCalibrationPointEnd()
        {
            CalibrationPointEndRequest cpe = new CalibrationPointEndRequest();

            Request(cpe);
        }

        public Object RequestCalibrationAbort()
        {
            RequestBase<ResponseBase> ca = new RequestBase<ResponseBase>();
            ca.Category = Protocol.CATEGORY_CALIBRATION;
            ca.Request = Protocol.CALIBRATION_REQUEST_ABORT;

            ca.AsyncLock = new Object();

            Request(ca);

            return ca.AsyncLock;
        }

        public void RequestCalibrationClear()
        {
            RequestBase<ResponseBase> cc = new RequestBase<ResponseBase>();
            cc.Category = Protocol.CATEGORY_CALIBRATION;
            cc.Request = Protocol.CALIBRATION_REQUEST_CLEAR;

            Request(cc);
        }

        public Object RequestScreenSwitch(int screenIndex, int screenResW, int screenResH, float screenPsyW, float screenPsyH)
        {
            TrackerSetRequest tsr = new TrackerSetRequest();

            tsr.Values.ScreenIndex = screenIndex;
            tsr.Values.ScreenResolutionWidth = screenResW;
            tsr.Values.ScreenResolutionHeight = screenResH;
            tsr.Values.ScreenPhysicalWidth = screenPsyW;
            tsr.Values.ScreenPhysicalHeight = screenPsyH;

            tsr.AsyncLock = new Object();

            Request(tsr);

            return tsr.AsyncLock;
        }

        public Object RequestFrame()
        {
            TrackerGetRequest tgr = new TrackerGetRequest();

            tgr.Values = new String[] { 
                Protocol.TRACKER_FRAME 
            };

            tgr.AsyncLock = new Object();

            Request(tgr);

            return tgr.AsyncLock;
        }

        public virtual ResponseBase ParseIncomingProcessResponse(JObject json, JToken value) { return null; }

        #endregion
    }

    internal class IncomingStreamHandler
    {
        private bool _IsRunning;
        private Thread _WorkerThread;
        private TcpClient _Socket;
        private StreamReader _Reader;
        private IGazeApiReponseListener _ResponseListener;
        private IGazeApiConnectionListener _ConnectionListener;
        private GazeApiManager _NetworkLayer;
        private ConcurrentDictionary<int, IRequest> _OnGoingRequests;

        public IncomingStreamHandler(
            TcpClient _socket, 
            IGazeApiReponseListener _responseListener, 
            IGazeApiConnectionListener _connectionListener,
            ConcurrentDictionary<int, IRequest> _onGoingRequests,
            GazeApiManager _networkLayer)
        {
            this._Socket = _socket;
            this._ResponseListener = _responseListener;
            this._ConnectionListener = _connectionListener;
            this._OnGoingRequests = _onGoingRequests;
            this._NetworkLayer = _networkLayer;
        }

        public void Start()
        {
            lock (this)
            {
                _IsRunning = true;
                ThreadStart ts = Work;
                _WorkerThread = new Thread(ts);
                _WorkerThread.Start();
            }
        }

        public void Stop()
        {
            lock (this)
            {
                _IsRunning = false;

                if (null != _WorkerThread)
                    _WorkerThread.Interrupt();
            }
        }

        private bool IsSocketConnected()
        {
            return !((null != _Socket && _Socket.Client.Poll(1000, SelectMode.SelectRead) && (_Socket.Client.Available == 0)) || !_Socket.Client.Connected);
        }

        private void Work()
        {
            try
            {
                _Reader = new StreamReader(_Socket.GetStream(), Encoding.ASCII);

                while (_IsRunning)
                {
                    while (!_Reader.EndOfStream /*&& IsSocketConnected()*/)
                    {
                        string responseJson = _Reader.ReadLine();

                        if (GazeManager.IS_DEBUG_MODE)
                            Debug.WriteLine("IN: " + responseJson);
                                
                        if (!String.IsNullOrEmpty(responseJson) && null != _ResponseListener)
                        {
                            JsonTextReader jsreader = new JsonTextReader(new StringReader(responseJson));
                            JObject json = (JObject)new JsonSerializer().Deserialize(jsreader);
                            JToken value;
                                
                            int id = 0;
                            if (json.TryGetValue(Protocol.KEY_ID, out value))
                                id = (int)value;

                            //get ongoing request if any
                            IRequest request;
                            _OnGoingRequests.TryRemove(id, out request);

                            //get status code
                            json.TryGetValue(Protocol.KEY_STATUSCODE, out value);

                            ResponseBase response = null;
                            if ((int)value == (int)HttpStatusCode.OK)
                            {
                                if(request != null)
                                {
                                    //matching request handles parsing
                                    response = (ResponseBase) request.ParseJsonResponse(json);
                                    response.TransitTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - request.TimeStamp;
                                }
                                else
                                {
                                    // Incoming message has no id and is a reponse to a process or a pushed gaze data frame
                                    json.TryGetValue(Protocol.KEY_CATEGORY, out value);
                                        
                                    if (value.ToObject<String>().Equals(Protocol.CATEGORY_CALIBRATION))
                                    {
                                        // response is calibration result
                                        response = json.ToObject<CalibrationPointEndResponse>();
                                    }
                                    else if (null != (response = _NetworkLayer.ParseIncomingProcessResponse(json, value)))
                                    { 
                                        // We allow the network layer extensions to optinally handle the process reponse
                                    }
                                    else
                                    {
                                        // response is gaze data frame
                                        response = json.ToObject<TrackerGetResponse>();
                                    }
                                }
                            }
                            else
                            {
                                //request failed
                                response = json.ToObject<ResponseFailed>();
                                response.Category = "";  //we reset category to simplify parsing logic

                                if(request != null)
                                    response.TransitTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - request.TimeStamp;
                            }

                            if (GazeManagerCore.IS_DEBUG_MODE && null != response && response.TransitTime != 0)
                                Debug.WriteLine("IN: transitTime " + response.TransitTime);

                            if (null != _ResponseListener)
                                _ResponseListener.OnGazeApiResponse(response, request);
                        }
                    }
                }
            }
            catch (ThreadInterruptedException tie)
            {
                Debug.WriteLine("Incoming stream handler interrupted: " + tie.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while establishing incoming socket connection: " + e.Message);

                if (GazeManager.IS_DEBUG_MODE)
                    Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                if(null != _Reader)
                    _Reader.Close();

                //connection has been lost
                _NetworkLayer.Close();
            }

            if (GazeManager.IS_DEBUG_MODE)
                Debug.WriteLine("IncommingStreamHandler closing down");
        }
    }

    internal class OutgoingStreamHandler
    {
        private readonly int NUM_WRITE_ATTEMPTS_BEFORE_FAIL = 3;
        private int numWriteAttempt;

        private bool _IsRunning;
        private Thread _WorkerThread;
        private TcpClient _Socket;
        private StreamWriter _Writer;
        private PriorityBlockingQueue<IRequest> _OutQueue;
        private ConcurrentDictionary<int, IRequest> _OngoingRequests;
        private IGazeApiConnectionListener _ConnectionListener;
        private GazeApiManager _NetworkLayer;

        public OutgoingStreamHandler(
            TcpClient socket,
            PriorityBlockingQueue<IRequest> queue,
            ConcurrentDictionary<int, IRequest> requests, 
            IGazeApiConnectionListener connectionListener, 
            GazeApiManager networkLayer)
        {
            _Socket = socket;
            _ConnectionListener = connectionListener;
            _NetworkLayer = networkLayer;
            _OutQueue = queue;
            _OngoingRequests = requests;
        }

        public void Start()
        {
            lock (this)
            {
                _IsRunning = true;
                ThreadStart ts = Work;
                _WorkerThread = new Thread(ts);
                _WorkerThread.Start();
            }
        }

        public void Stop()
        {
            lock (this)
            {
                _IsRunning = false;

                if (null != _WorkerThread)
                    _WorkerThread.Interrupt();
            }
        }

        private void Work()
        {
            try
            {
                IRequest request = null;
                String requestJson = string.Empty;
                
                _Writer = new StreamWriter(_Socket.GetStream(), Encoding.ASCII);

                //while waiting for queue to populate and thread not killed
                while (_IsRunning)
                {
                    request = _OutQueue.Dequeue();

                    if (request.IsCancelled)
                        continue;

                    request.TimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    requestJson = request.ToJsonString();
                        
                	while(true)
                	{
                        try
                        {
                            _Writer.WriteLine(requestJson);
                            _Writer.Flush();

                            _OngoingRequests.TryAdd(request.Id, request);

                            if(GazeManager.IS_DEBUG_MODE)
                                Debug.WriteLine("OUT: " + requestJson);

                            break;
                        }
                        catch (IOException ioe)
                        {
                            // Has writing to socket failed and may server be disconnected?
                            if (++request.RetryAttempts >= NUM_WRITE_ATTEMPTS_BEFORE_FAIL)
                            {
                                request.Finish();
                                IRequest value;
                                _OngoingRequests.TryRemove(request.Id, out value);
                                throw new Exception("OutgoingStreamHandler failed writing to stream despite several retires");
                            }
                               
                        	if(GazeManager.IS_DEBUG_MODE)
                        	{
                                Debug.WriteLine("OutgoingStreamHandler IO exception: " + ioe.Message);
                                Debug.WriteLine(ioe.StackTrace);
                        	}
                        }
                    }
                }
            }
            catch (ThreadInterruptedException tie)
            {
                if (GazeManager.IS_DEBUG_MODE)
                    Debug.WriteLine("Outgoing stream handler interrupted: " + tie.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while establishing outgoing socket connection: " + e.Message);

                if (GazeManager.IS_DEBUG_MODE)
                    Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                try
                {
                    if (null != _Writer)
                        _Writer.Close();
                }
                catch (Exception e)
                {
                    // consume
                }

                //connection has been lost
                _NetworkLayer.Close();
            }

            if (GazeManager.IS_DEBUG_MODE)
                Debug.WriteLine("OutgoingStreamHandler closing down");
        }
    }

    //<summary>
    // Callback interface responsible for handling messaages returned from the GazeApiManager 
    // </summary>
    public interface IGazeApiReponseListener
    {
        void OnGazeApiResponse(ResponseBase response, IRequest request);
    }

    //<summary>
    // Callback interface responsible for handling connection state notifications from the GazeApiManager
    // </summary>
    public interface IGazeApiConnectionListener
    {
        void OnGazeApiConnectionStateChanged(bool isConnected);
    }
}
