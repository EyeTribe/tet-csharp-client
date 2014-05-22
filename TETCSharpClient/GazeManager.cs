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
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using TETCSharpClient.Data;
using TETCSharpClient.Reply;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace TETCSharpClient
{
    /// <summary>
    /// This singleton is the main entry point of the TET C# Client. It manages all routines associated to gaze control.
    /// Using this class a developer can 'calibrate' an eye tracking setup and attach listeners to recieve live data streams
    /// of <see cref="TETCSharpClient.Data.GazeData"/> updates.
    /// </summary>
    public class GazeManager : IGazeApiReponseListener, IGazeApiConnectionListener
    {
        #region Constants

        public const int INIT_TIME_DELAY_SECONDS = 10;

        public const int FRAME_QUEUE_SIZE = 10;

        #endregion

        #region Enums

        /// <summary>
        /// The possible states of the Tracer device
        /// </summary>
        public enum TrackerState
        {
            TRACKER_CONNECTED = 0,
            TRACKER_NOT_CONNECTED = 1,
            TRACKER_CONNECTED_BADFW = 2,
            TRACKER_CONNECTED_NOUSB3 = 3,
            TRACKER_CONNECTED_NOSTREAM = 4
        }

        /// <summary>
        /// The possible frame grabbing modes of the Tracker Server
        /// </summary>
        public enum ClientMode
        {
            Push = 1001,
            Pull = 1002
        }

        /// <summary>
        /// The possible frame rates of the Tracker Server
        /// </summary>
        public enum FrameRate
        {
            FPS_30 = 30,
            FPS_60 = 60
        }

        /// <summary>
        /// The EyeTribe API compliance levels
        /// </summary>
        public enum ApiVersion
        {
            VERSION_1_0 = 1
        }

        #endregion

        #region Variables

        private static GazeManager instance;
        internal GazeApiManager apiManager;
        private WaitHandleWrap events;

        protected bool isActive;

        protected static readonly Object initializationLock = new Object();
        protected static bool isInizializing;

        internal Heartbeat heartbeatHandler;
        private GazeBroadcaster gazeBroadcaster;
        private FixedSizeQueue<GazeData> queueGazeData;

        protected int sampledCalibrationPoints;
        protected int totalCalibrationPoints;

        protected List<IGazeListener> gazeListeners;
        protected List<ICalibrationResultListener> calibrationResultListeners;
        protected List<ITrackerStateListener> trackerStateListeners;
        protected List<IConnectionStateListener> connectionStateListeners;
        protected ICalibrationProcessHandler calibrationListener;

        #endregion

        #region Constructor

        private GazeManager()
        {
            HeartbeatMillis = 3000; //default value
            gazeListeners = new List<IGazeListener>();
            calibrationResultListeners = new List<ICalibrationResultListener>();
            trackerStateListeners = new List<ITrackerStateListener>();
            connectionStateListeners = new List<IConnectionStateListener>();
            queueGazeData = new FixedSizeQueue<GazeData>(FRAME_QUEUE_SIZE);
        }

        #endregion

        #region Get/Set

        public static GazeManager Instance
        {
            get { return instance ?? (instance = new GazeManager()); }
        }

        /// <summary>
        /// Is the client library connected to Tracker Server?
        /// </summary>
        [Obsolete("Deprecated, use IsActivated() instead", false)]
        public bool IsConnected
        {
            get { return isActive; }
        }

        /// <summary>
        /// Is the client library connected to Tracker Server and initialized?
        /// </summary>
        public bool IsActivated
        {
            get { return (null != apiManager ? apiManager.IsConnected() : false) && isActive; }
        }

        /// <summary>
        /// The current state of the connected TrackerDevice.
        /// </summary>
        public TrackerState Trackerstate { get; private set; }

        /// <summary>
        /// The lastest performed and valid CalibrationResult. Note the result is not nessesarily positive
        /// and clients should evaluate the result before using. 
        /// </summary>
        public CalibrationResult LastCalibrationResult { get; private set; }

        /// <summary>
        /// Is the client in the middle of a calibration process?
        /// </summary>
        public Boolean IsCalibrating { get; private set; }

        /// <summary>
        /// Is the client already calibrated?
        /// </summary>
        public Boolean IsCalibrated { get; private set; }

        /// <summary>
        /// Index of currently used screen. Used for multiscreen setups.
        /// </summary>
        public int ScreenIndex { get; private set; }

        /// <summary>
        /// Width of screen resolution in pixels
        /// </summary>
        public int ScreenResolutionWidth { get; private set; }

        /// <summary>
        /// Height of screen resolution in pixels
        /// </summary>
        public int ScreenResolutionHeight { get; private set; }

        /// <summary>
        /// Physical width of screen in meters
        /// </summary>
        public float ScreenPhysicalWidth { get; private set; }

        /// <summary>
        /// Physical height of screen in meters
        /// </summary>
        public float ScreenPhysicalHeight { get; private set; }

        /// <summary>
        /// Length of a heartbeat in milliseconds. 
        /// The Tracker Server defines the desired length of a heartbeat and is in
        /// this implementation automatically acquired through the Tracker API.
        /// </summary>
        internal int HeartbeatMillis { get; private set; }

        /// <summary>
        /// Number of frames per second delivered by Tracker Server
        /// </summary>
        public FrameRate Framerate { get; private set; }

        /// <summary>
        /// Current API version compliance of Tracker Server
        /// </summary>
        protected ApiVersion version { get; private set; }

        /// <summary>
        /// Current running mode of this client
        /// </summary>
        protected ClientMode clientMode { get; private set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Internal callback method. Should not be called directly.
        /// </summary>
        public void OnGazeApiResponse(String response)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(HandleApiResponse), response);
        }

        internal void HandleApiResponse(Object stateInfo)
        {
            try
            {
                String response = (String)stateInfo;

                var reply = JsonConvert.DeserializeObject<ReplyBase>(response);

                if (reply.StatusCode == (int)HttpStatusCode.OK)
                {
                    switch (reply.Category)
                    {
                        case Protocol.CATEGORY_TRACKER:

                            if (reply.Request.Equals(Protocol.TRACKER_REQUEST_GET))
                            {
                                var jsreader = new JsonTextReader(new StringReader(response));
                                var json = (JObject)new JsonSerializer().Deserialize(jsreader);

                                JObject values = null != json[Protocol.KEY_VALUES] ? json[Protocol.KEY_VALUES].ToObject<JObject>() : null;
                                JToken value;

                                if (null != values)
                                {
                                    if (values.TryGetValue(Protocol.TRACKER_VERSION, out value))
                                        version = value.ToObject<ApiVersion>();

                                    if (values.TryGetValue(Protocol.TRACKER_MODE_PUSH, out value))
                                    {
                                        if ((bool)value)
                                            clientMode = ClientMode.Push;
                                        else
                                            clientMode = ClientMode.Pull;
                                    }

                                    if (values.TryGetValue(Protocol.TRACKER_HEARTBEATINTERVAL, out value))
                                        HeartbeatMillis = (int)value;

                                    if (values.TryGetValue(Protocol.TRACKER_FRAMERATE, out value))
                                        Framerate = value.ToObject<FrameRate>();

                                    if (values.TryGetValue(Protocol.TRACKER_TRACKERSTATE, out value))
                                    {
                                        //if tracker state changed, notify listeners
                                        if ((int)value != (int)Convert.ChangeType(Trackerstate, Trackerstate.GetTypeCode()))
                                        {
                                            Trackerstate = (TrackerState)(int)value;

                                            lock (((ICollection)trackerStateListeners).SyncRoot)
                                            {
                                                foreach (ITrackerStateListener listener in trackerStateListeners)
                                                {
                                                    ThreadPool.QueueUserWorkItem(new WaitCallback(HandleOnTrackerStateChanged), new Object[] { listener, Trackerstate });
                                                }
                                            }
                                        }
                                    }

                                    if (values.TryGetValue(Protocol.TRACKER_ISCALIBRATING, out value))
                                        IsCalibrating = (bool)value;

                                    if (values.TryGetValue(Protocol.TRACKER_ISCALIBRATED, out value))
                                        IsCalibrated = (bool)value;

                                    if (values.TryGetValue(Protocol.TRACKER_CALIBRATIONRESULT, out value))
                                    {
                                        //is result different from current?
                                        if (null == LastCalibrationResult || !LastCalibrationResult.Equals(value.ToObject<CalibrationResult>()))
                                        {
                                            LastCalibrationResult = value.ToObject<CalibrationResult>();

                                            lock (((ICollection)calibrationResultListeners).SyncRoot)
                                            {
                                                foreach (ICalibrationResultListener listener in calibrationResultListeners)
                                                {
                                                    ThreadPool.QueueUserWorkItem(new WaitCallback(HandleOnCalibrationChanged), new Object[] { listener, IsCalibrated, LastCalibrationResult });
                                                }
                                            }
                                        }
                                    }

                                    if (values.TryGetValue(Protocol.TRACKER_SCREEN_RESOLUTION_WIDTH, out value))
                                        ScreenResolutionWidth = (int)value;

                                    if (values.TryGetValue(Protocol.TRACKER_SCREEN_RESOLUTION_HEIGHT, out value))
                                        ScreenResolutionHeight = (int)value;

                                    if (values.TryGetValue(Protocol.TRACKER_SCREEN_PHYSICAL_WIDTH, out value))
                                        ScreenPhysicalWidth = (float)value;

                                    if (values.TryGetValue(Protocol.TRACKER_SCREEN_PHYSICAL_HEIGHT, out value))
                                        ScreenPhysicalHeight = (float)value;

                                    if (values.TryGetValue(Protocol.TRACKER_SCREEN_INDEX, out value))
                                    {
                                        //if screen index changed, notify listeners
                                        if ((int)value != ScreenIndex)
                                        {
                                            ScreenIndex = (int)value;

                                            lock (((ICollection)trackerStateListeners).SyncRoot)
                                            {
                                                foreach (ITrackerStateListener listener in trackerStateListeners)
                                                {
                                                    ThreadPool.QueueUserWorkItem(new WaitCallback(HandleOnScreenStatesChanged), new Object[] { listener, ScreenIndex, ScreenResolutionWidth, ScreenResolutionHeight, ScreenPhysicalWidth, ScreenPhysicalHeight });
                                                }
                                            }
                                        }
                                    }

                                    if (values.TryGetValue(Protocol.TRACKER_FRAME, out value) && null != gazeBroadcaster)
                                    {
                                        GazeData gd = value.ToObject<GazeData>();

                                        //fixing timestamp based on string representation, Json 32bit int issue
                                        if (null != gd.TimeStampString && !String.IsNullOrEmpty(gd.TimeStampString))
                                        {
                                            try
                                            {
                                                DateTime dt = Convert.ToDateTime(gd.TimeStampString);
                                                gd.TimeStamp = (long)(dt - new DateTime(1970, 1, 1)).TotalMilliseconds; //UTC
                                            }
                                            catch (Exception e)
                                            {
                                                //consume possible error
                                            }
                                        }

                                        //Add gaze update to high frequency broadcasting queue
                                        lock (((ICollection)queueGazeData).SyncRoot)
                                        {
                                            queueGazeData.Enqueue(value.ToObject<GazeData>());
                                        }

                                        events.GetUpdateHandle().Set();
                                    }
                                }

                                //Handle initialization
                                if (isInizializing)
                                {
                                    lock (initializationLock)
                                    {
                                        isInizializing = false;
                                        Monitor.Pulse(initializationLock);
                                    }
                                }
                            }
                            else if (reply.Request.Equals(Protocol.TRACKER_REQUEST_SET))
                            {
                                //Do nothing
                            }
                            break;

                        case Protocol.CATEGORY_CALIBRATION:

                            switch (reply.Request)
                            {
                                case Protocol.CALIBRATION_REQUEST_START:

                                    IsCalibrating = true;

                                    if (null != calibrationListener)
                                        //Notify calibration listener that a new calibration process was successfully started
                                        try
                                        {
                                            calibrationListener.OnCalibrationStarted();
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Exception while calling ICalibrationProcessHandler.OnCalibrationStarted() on listener " + calibrationListener + ": " + e.StackTrace);
                                        }

                                    break;

                                case Protocol.CALIBRATION_REQUEST_POINTSTART:
                                    break;

                                case Protocol.CALIBRATION_REQUEST_POINTEND:

                                    ++sampledCalibrationPoints;

                                    if (null != calibrationListener)
                                    {
                                        //Notify calibration listener that a new calibration point has been sampled
                                        try
                                        {
                                            calibrationListener.OnCalibrationProgress(sampledCalibrationPoints / totalCalibrationPoints);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Exception while calling ICalibrationProcessHandler.OnCalibrationProgress() on listener " + calibrationListener + ": " + e.StackTrace);
                                        }


                                        if (sampledCalibrationPoints == totalCalibrationPoints)
                                            //Notify calibration listener that all calibration points have been sampled and the analysis of the calirbation results has begun 
                                            try
                                            {
                                                calibrationListener.OnCalibrationProcessing();
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception while calling ICalibrationProcessHandler.OnCalibrationProcessing() on listener " + calibrationListener + ": " + e.StackTrace);
                                            }
                                    }

                                    var cper = JsonConvert.DeserializeObject<CalibrationPointEndReply>(response);

                                    if (cper == null || cper.Values.CalibrationResult == null)
                                        break; // not done with calibration yet

                                    IsCalibrated = cper.Values.CalibrationResult.Result;
                                    IsCalibrating = !cper.Values.CalibrationResult.Result;
                                    LastCalibrationResult = cper.Values.CalibrationResult;

                                    if (null != calibrationListener)
                                    {
                                        //Notify calibration listener that calibration results are ready for evaluation
                                        try
                                        {
                                            calibrationListener.OnCalibrationResult(cper.Values.CalibrationResult);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Exception while calling ICalibrationProcessHandler.OnCalibrationResult() on listener " + calibrationListener + ": " + e.StackTrace);
                                        }
                                    }
                                    break;

                                case Protocol.CALIBRATION_REQUEST_ABORT:
                                    IsCalibrating = false;

                                    //restore states of last calibration if any
                                    apiManager.RequestCalibrationStates();
                                    break;

                                case Protocol.CALIBRATION_REQUEST_CLEAR:
                                    IsCalibrated = false;
                                    IsCalibrating = false;
                                    LastCalibrationResult = null;
                                    break;
                            }
                            break; // end calibration switch

                        case Protocol.CATEGORY_HEARTBEAT:
                            //do nothing
                            break;

                        default:
                            var rf = JsonConvert.DeserializeObject<ReplyFailed>(response);
                            Debug.WriteLine("Request FAILED");
                            Debug.WriteLine("Category: " + rf.Category);
                            Debug.WriteLine("Request: " + rf.Request);
                            Debug.WriteLine("StatusCode: " + rf.StatusCode);
                            Debug.WriteLine("StatusMessage: " + rf.Values.StatusMessage);
                            break;
                    }
                }
                else
                {
                    var rf = JsonConvert.DeserializeObject<ReplyFailed>(response);

                    /* 
                     * JSON Message status code is different from HttpStatusCode.OK. Check if special TET 
                     * specific statuscode before handling error 
                     */

                    switch (rf.StatusCode)
                    {
                        case Protocol.STATUSCODE_CALIBRATION_UPDATE:
                            //The calibration state has changed, clients should update themselves
                            apiManager.RequestCalibrationStates();
                            break;

                        case Protocol.STATUSCODE_SCREEN_UPDATE:
                            //The primary screen index has changed, clients should update themselves
                            apiManager.RequestScreenStates();
                            break;

                        case Protocol.STATUSCODE_TRACKER_UPDATE:
                            //The connected Tracker Device has changed state, clients should update themselves
                            apiManager.RequestTrackerState();
                            break;

                        default:
                            Debug.WriteLine("Request FAILED");
                            Debug.WriteLine("Category: " + rf.Category);
                            Debug.WriteLine("Request: " + rf.Request);
                            Debug.WriteLine("StatusCode: " + rf.StatusCode);
                            Debug.WriteLine("StatusMessage: " + rf.Values.StatusMessage);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while parsing API response: " + e.StackTrace);
            }
        }

        /// <summary>
        /// Internal delegate helper method. Used fro ThreadPooling.
        /// </summary>
        internal static void HandleOnTrackerStateChanged(Object stateInfo)
        {
            ITrackerStateListener listener = null;
            try
            {
                Object[] objs = (Object[])stateInfo;
                listener = (ITrackerStateListener)objs[0];
                TrackerState state = (TrackerState)objs[1];
                listener.OnTrackerStateChanged(state);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while calling ITrackerStateListener.OnTrackerStateChanged() on listener " + listener + ": " + e.StackTrace);
            }
        }

        /// <summary>
        /// Internal delegate helper method. Used fro ThreadPooling.
        /// </summary>
        internal static void HandleOnCalibrationChanged(Object stateInfo)
        {
            ICalibrationResultListener listener = null;
            try
            {
                Object[] objs = (Object[])stateInfo;
                listener = (ICalibrationResultListener)objs[0];
                Boolean isCalibrated = Convert.ToBoolean(objs[1]);
                CalibrationResult lastResult = (CalibrationResult)objs[2];
                listener.OnCalibrationChanged(isCalibrated, lastResult);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while calling ICalibrationResultListener.OnCalibrationChanged() on listener " + listener + ": " + e.StackTrace);
            }
        }

        /// <summary>
        /// Internal delegate helper method. Used fro ThreadPooling.
        /// </summary>
        internal static void HandleOnScreenStatesChanged(Object stateInfo)
        {
            ITrackerStateListener listener = null;
            try
            {
                Object[] objs = (Object[])stateInfo;
                listener = (ITrackerStateListener)objs[0];
                Int32 screenIndex = Convert.ToInt32(objs[1]);
                Int32 screenResolutionWidth = Convert.ToInt32(objs[2]);
                Int32 screenResolutionHeight = Convert.ToInt32(objs[3]);
                Double screenPhysicalWidth = Convert.ToDouble(objs[4]);
                Double screenPhysicalHeight = Convert.ToDouble(objs[5]);
                listener.OnScreenStatesChanged(screenIndex, screenResolutionWidth, screenResolutionHeight, (float)screenPhysicalWidth, (float)screenPhysicalHeight);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while calling ITrackerStateListener.OnScreenStatesChanged() on listener " + listener + ": " + e.StackTrace);
            }
        }

        /// <summary>
        /// Internal callback method. Should not be called directly.
        /// </summary>
        public void OnGazeApiConnectionStateChanged(bool isConnected)
        {
            Debug.WriteLine("isConnected: " + isConnected);

            lock (((ICollection)connectionStateListeners).SyncRoot)
            {
                foreach (IConnectionStateListener listener in connectionStateListeners)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(HandleOnConnectionStateChanged), new Object[] { listener, isConnected });
                }
            }
        }

        /// <summary>
        /// Internal delegate helper method. Used fro ThreadPooling.
        /// </summary>
        internal static void HandleOnConnectionStateChanged(Object stateInfo)
        {
            IConnectionStateListener listener = null;
            try
            {
                Object[] objs = (Object[])stateInfo;
                listener = (IConnectionStateListener)objs[0];
                Boolean isConnected = (Boolean)objs[1];
                listener.OnConnectionStateChanged(isConnected);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while calling IConnectionStateListener.OnConnectionStateChanged() on listener " + listener + ": " + e.StackTrace);
            }
        }

        /// <summary>
        /// Activates TET C# Client and all underlying routines using default values. Should be called _only_ 
        /// once when an application starts up. Calling thread will be locked during
        /// initialization.
        /// </summary>
        /// <param name="apiVersion">Version number of the Tracker API that this client will be compliant to</param>
        /// <param name="mode">Mode though which the client will receive GazeData. Either ClientMode.Push or ClientMode.Pull</param>
        /// <returns>True if succesfully activated, false otherwise</returns>
        public bool Activate(ApiVersion apiVersion, ClientMode mode)
        {
            return Activate(apiVersion, mode, GazeApiManager.DEFAULT_SERVER_HOST, GazeApiManager.DEFAULT_SERVER_PORT);
        }

        /// <summary>
        /// Activates TET C# Client and all underlying routines. Should be called _only_ 
        /// once when an application starts up. Calling thread will be locked during
        /// initialization.
        /// </summary>
        /// <param name="apiVersion">Version number of the Tracker API that this client will be compliant to</param>
        /// <param name="mode">Mode though which the client will receive GazeData. Either ClientMode.Push or ClientMode.Pull</param>
        /// <param name="hostname">The host name or IP address where the eye tracking server is running.</param>
        /// <param name="portnumber">The port number used for the eye tracking server</param>
        /// <returns>True if succesfully activated, false otherwise</returns>
        public bool Activate(ApiVersion apiVersion, ClientMode mode, string hostname, int portnumber)
        {
            //lock to ensure that state changing method calls are synchronous
            lock (instance)
            {
                //if already running, deactivate before starting anew
                if (IsActivated)
                    Deactivate();

                Object threadLock = Thread.CurrentThread;

                //lock calling thread while initializing
                lock (threadLock)
                {
                    //only one entity can initialize at the time
                    lock (initializationLock)
                    {
                        if (!IsActivated)
                        {
                            isInizializing = true;

                            //establish connection in seperate thread
                            apiManager = new GazeApiManager(this, this);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(HandleServerConnect), new Object[] { apiManager, mode, apiVersion, hostname, portnumber });

                            //We wait untill above requests have been handled by server or timeout occurs
                            bool waitSuccess = Monitor.Wait(initializationLock, TimeSpan.FromSeconds(INIT_TIME_DELAY_SECONDS));

                            if (!waitSuccess)
                            {
                                Deactivate();
                                Debug.WriteLine("Error initializing GazeManager, is EyeTribe Server running?");
                            }
                            else
                            {
                                //init heartbeat
                                heartbeatHandler = new Heartbeat(apiManager);
                                heartbeatHandler.Start();

                                isActive = true;
                            }
                        }

                        return IsActivated;
                    }
                }
            }
        }

        /// <summary>
        /// Internal delegate helper method. Used fro ThreadPooling.
        /// </summary>
        internal static void HandleServerConnect(Object stateInfo)
        {
            try
            {
                Object[] objs = (Object[])stateInfo;
                GazeApiManager apiManager = (GazeApiManager)objs[0];
                ClientMode mode = (ClientMode)objs[1];
                ApiVersion version = (ApiVersion)objs[2];
                string hostname = Convert.ToString(objs[3]);
                int portnumber = Convert.ToInt32(objs[4]);

                apiManager.Connect(hostname, portnumber);

                if (apiManager.IsConnected())
                {
                    apiManager.RequestTracker(mode, version);
                    apiManager.RequestAllStates();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while connecting to EyeTribe Server: " + e.StackTrace);
            }
        }

        /// <summary>
        /// Deactivates TET C# Client and all under lying routines. Should be called when
        /// a application closes down.
        /// </summary>
        public void Deactivate()
        {
            //lock to ensure that state changing method calls are synchronous
            lock (instance)
            {
                if (null != heartbeatHandler)
                {
                    heartbeatHandler.Stop();
                    heartbeatHandler = null;
                }

                if (null != apiManager)
                {
                    apiManager.Close();
                    apiManager = null;
                }

                //clearing listeners will stop heartbeat and broadcasting threads
                ClearListeners();

                isActive = false;
                isInizializing = false;
            }
        }

        /// <summary>
        /// Adds a <see cref="TETCSharpClient.IGazeListener"/> to the TET C# client. This listener 
        /// will recieve <see cref="TETCSharpClient.Data.GazeData"/> updates when available
        /// </summary>
        /// <param name="listener">The <see cref="TETCSharpClient.IGazeListener"/> instance to add</param>
        public void AddGazeListener(IGazeListener listener)
        {
            if (null != listener)
            {
                lock (((ICollection)gazeListeners).SyncRoot)
                {
                    if (gazeListeners.Count == 0)
                    {
                        //init wait handles
                        events = new WaitHandleWrap();

                        //init broadcasting routines, 
                        gazeBroadcaster = new GazeBroadcaster(queueGazeData, gazeListeners, events);
                        gazeBroadcaster.Start();
                    }

                    if (!gazeListeners.Contains(listener))
                        gazeListeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// Remove a <see cref="TETCSharpClient.IGazeListener"/> from the TET C# client.
        /// </summary>
        /// <returns>True if succesfully removed, false otherwise</returns>
        /// <param name="listener">The <see cref="TETCSharpClient.IGazeListener"/> instance to remove</param>
        public bool RemoveGazeListener(IGazeListener listener)
        {
            bool result = false;

            lock (((ICollection)gazeListeners).SyncRoot)
            {
                if (gazeListeners.Contains(listener))
                    result = gazeListeners.Remove(listener);

                if (gazeListeners.Count == 0)
                {
                    if (null != gazeBroadcaster)
                    {
                        gazeBroadcaster.Stop();
                        gazeBroadcaster = null;
                    }

                    lock (((ICollection)queueGazeData).SyncRoot)
                    {
                        queueGazeData.Clear();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets current number of attached <see cref="TETCSharpClient.IGazeListener"/> instances.
        /// </summary>
        /// <returns>Curent number of listeners</returns>
        public int GetNumGazeListeners()
        {
            if (null != gazeListeners)
                return gazeListeners.Count;

            return -1;
        }

        /// <summary>
        /// Checkes if a given instance of <see cref="TETCSharpClient.IGazeListener"/> is currently attached.
        /// </summary>
        /// <returns>True if already attached, false otherwise</returns>
        public bool HasGazeListener(IGazeListener listener)
        {
            bool result = false;

            lock (((ICollection)gazeListeners).SyncRoot)
            {
                result = gazeListeners.Contains(listener);
            }

            return result;
        }

        /// <summary>
        /// Adds a <see cref="ICalibrationResultListener"/> to the TET C# client. This listener 
        /// will recieve updates about calibration state changes.
        /// </summary>
        /// <param name="listener">The <see cref="TETCSharpClient.ICalibrationResultListener"/> instance to add</param>
        public void AddCalibrationResultListener(ICalibrationResultListener listener)
        {
            if (null != listener)
            {
                lock (((ICollection)calibrationResultListeners).SyncRoot)
                {
                    if (!calibrationResultListeners.Contains(listener))
                        calibrationResultListeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// Remove a <see cref="TETCSharpClient.ICalibrationResultListener"/> from the TET C# client.
        /// </summary>
        /// <returns>True if succesfully removed, false otherwise</returns>
        /// <param name="listener">The <see cref="TETCSharpClient.ICalibrationResultListener"/> instance to remove</param>
        public bool RemoveCalibrationResultListener(ICalibrationResultListener listener)
        {
            bool result = false;

            lock (((ICollection)calibrationResultListeners).SyncRoot)
            {
                if (calibrationResultListeners.Contains(listener))
                    result = calibrationResultListeners.Remove(listener);
            }

            return result;
        }

        /// <summary>
        /// Gets current number of attached <see cref="TETCSharpClient.ICalibrationResultListener"/> instances.
        /// </summary>
        /// <returns>Curent number of listeners</returns>
        public int GetNumCalibrationResultListeners()
        {
            if (null != calibrationResultListeners)
                return calibrationResultListeners.Count;

            return -1;
        }

        /// <summary>
        /// Checkes if a given instance of <see cref="TETCSharpClient.ICalibrationResultListener"/> is currently attached.
        /// </summary>
        /// <returns>True if already attached, false otherwise</returns>
        public bool HasCalibrationResultListener(ICalibrationResultListener listener)
        {
            bool result = false;

            lock (((ICollection)calibrationResultListeners).SyncRoot)
            {
                result = calibrationResultListeners.Contains(listener);
            }

            return result;
        }

        /// <summary>
        /// Adds a <see cref="TETCSharpClient.ITrackerStateListener"/> to the TET C# client. This listener 
        /// will recieve updates about change of active screen index.
        /// </summary>
        /// <param name="listener">The <see cref="TETCSharpClient.ITrackerStateListener"/> instance to add</param>
        public void AddTrackerStateListener(ITrackerStateListener listener)
        {
            if (null != listener)
            {
                lock (((ICollection)trackerStateListeners).SyncRoot)
                {
                    if (!trackerStateListeners.Contains(listener))
                        trackerStateListeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// Remove a <see cref="TETCSharpClient.ITrackerStateListener"/> from the TET C# client.
        /// </summary>
        /// <returns>True if succesfully removed, false otherwise</returns>
        /// <param name="listener">The <see cref="TETCSharpClient.ITrackerStateListener"/> instance to remove</param>
        public bool RemoveTrackerStateListener(ITrackerStateListener listener)
        {
            bool result = false;

            lock (((ICollection)trackerStateListeners).SyncRoot)
            {
                if (trackerStateListeners.Contains(listener))
                    result = trackerStateListeners.Remove(listener);
            }

            return result;
        }

        /// <summary>
        /// Gets current number of attached <see cref="TETCSharpClient.ITrackerStateListener"/> instances.
        /// </summary>
        /// <returns>Curent number of listeners</returns>
        public int GetNumTrackerStateListeners()
        {
            if (null != trackerStateListeners)
                return trackerStateListeners.Count;

            return -1;
        }

        /// <summary>
        /// Checkes if a given instance of <see cref="TETCSharpClient.ITrackerStateListener"/> is currently attached.
        /// </summary>
        /// <returns>True if already attached, false otherwise</returns>
        public bool HasTrackerStateListener(ITrackerStateListener listener)
        {
            bool result = false;

            lock (((ICollection)trackerStateListeners).SyncRoot)
            {
                result = trackerStateListeners.Contains(listener);
            }

            return result;
        }

        /// <summary>
        /// Adds a <see cref="TETCSharpClient.IConnectionStateListener"/> to the TET C# client. This listener 
        /// will recieve updates about change in connection state to the EyeTribe Server.
        /// </summary>
        /// <param name="listener">The <see cref="TETCSharpClient.IConnectionStateListener"/> instance to add</param>
        public void AddConnectionStateListener(IConnectionStateListener listener)
        {
            if (null != listener)
            {
                lock (((ICollection)connectionStateListeners).SyncRoot)
                {
                    if (!connectionStateListeners.Contains(listener))
                        connectionStateListeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// Remove a <see cref="TETCSharpClient.IConnectionStateListener"/> from the TET C# client.
        /// </summary>
        /// <returns>True if succesfully removed, false otherwise</returns>
        /// <param name="listener">The <see cref="TETCSharpClient.IConnectionStateListener"/> instance to remove</param>
        public bool RemoveConnectionStateListener(IConnectionStateListener listener)
        {
            bool result = false;

            lock (((ICollection)connectionStateListeners).SyncRoot)
            {
                if (connectionStateListeners.Contains(listener))
                    result = connectionStateListeners.Remove(listener);
            }

            return result;
        }

        /// <summary>
        /// Gets current number of attached <see cref="TETCSharpClient.IConnectionStateListener"/> instances.
        /// </summary>
        /// <returns>Curent number of listeners</returns>
        public int GetNumConnectionStateListeners()
        {
            if (null != connectionStateListeners)
                return connectionStateListeners.Count;

            return -1;
        }

        /// <summary>
        /// Checkes if a given instance of <see cref="TETCSharpClient.IConnectionStateListener"/> is currently attached.
        /// </summary>
        /// <returns>True if already attached, false otherwise</returns>
        public bool HasConnectionStateListener(IConnectionStateListener listener)
        {
            bool result = false;

            lock (((ICollection)connectionStateListeners).SyncRoot)
            {
                result = connectionStateListeners.Contains(listener);
            }

            return result;
        }

        /// <summary>
        /// Clear all attached listeners, clears GazeData queue and stop broadcating
        /// </summary>
        public void ClearListeners()
        {
            if (null != gazeListeners)
                lock (((ICollection)gazeListeners).SyncRoot)
                {
                    gazeListeners.Clear();
                }

            if (null != calibrationResultListeners)
                lock (((ICollection)calibrationResultListeners).SyncRoot)
                {
                    calibrationResultListeners.Clear();
                }

            if (null != trackerStateListeners)
                lock (((ICollection)trackerStateListeners).SyncRoot)
                {
                    trackerStateListeners.Clear();
                }

            if (null != connectionStateListeners)
                lock (((ICollection)connectionStateListeners).SyncRoot)
                {
                    connectionStateListeners.Clear();
                }

            if (null != queueGazeData)
                lock (((ICollection)queueGazeData).SyncRoot)
                {
                    queueGazeData.Clear();
                }

            if (null != gazeBroadcaster)
            {
                gazeBroadcaster.Stop();
                gazeBroadcaster = null;
            }
        }

        /// <summary>
        /// Switch currently active screen. Enabled the user to take control of which screen is used for calibration 
        /// and gaze control.
        /// </summary>
        /// <param name="screenIndex">Index of nex screen. On windows 'Primary Screen' has index 0.</param>
        /// <param name="screenResW">Screen resolution width in pixels</param>
        /// <param name="screenResH">Screen resolution height in pixels</param>
        /// <param name="screenPsyW">Physical Screen width in meters</param>
        /// <param name="screenPsyH">Physical Screen height in meters</param>
        public void SwitchScreen(int screenIndex, int screenResW, int screenResH, float screenPsyW, float screenPsyH)
        {
            if (IsActivated)
            {
                apiManager.RequestScreenSwitch(screenIndex, screenResW, screenResH, screenPsyW, screenPsyH);
            }
            else
                Debug.WriteLine("TET C# Client not activated!");
        }

        /// <summary>
        /// Initiate a new calibration process. Must be called before any call to <see cref="TETCSharpClient.GazeManager.CalibrationPointStart(int, int)"/> 
        /// or <see cref="TETCSharpClient.GazeManager.CalibrationPointEnd()"/> .
        /// <para/>
        /// Any previous (and possible running) calibration process must be completed or aborted before calling this.
        /// <para/>
        /// A full calibration process consists of a number of calls to <see cref="TETCSharpClient.GazeManager.CalibrationPointStart(int, int)"/> 
        /// and <see cref="TETCSharpClient.GazeManager.CalibrationPointEnd()"/>  matching the total number of clibration points set by the
        /// numCalibrationPoints parameter.
        /// </summary>
        /// <param name="numCalibrationPoints">The number of calibration points that will be used in this calibration</param>
        /// <param name="listener">The <see cref="TETCSharpClient.ICalibrationProcessHandler"/>  instance that will receive callbacks during the 
        /// calibration process</param>
        public void CalibrationStart(short numCalibrationPoints, ICalibrationProcessHandler listener)
        {
            if (IsActivated)
            {
                if (!IsCalibrating)
                {
                    sampledCalibrationPoints = 0;
                    totalCalibrationPoints = numCalibrationPoints;
                    calibrationListener = listener;
                    apiManager.RequestCalibrationStart(numCalibrationPoints);
                }
                else
                    Debug.WriteLine(
                        "Calibration process already running. Call CalibrationAbort() before starting new process");
            }
            else
                Debug.WriteLine("TET C# Client not activated!");
        }

        /// <summary>
        /// Called for every calibration point during a calibration process. This call should be followed by a call to
        /// <see cref="TETCSharpClient.GazeManager.CalibrationPointEnd()"/>  1-2 seconds later.
        /// <para/>
        /// The calibration process must be initiated by a call to <see cref="TETCSharpClient.GazeManager.CalibrationStart(short, ICalibrationProcessHandler)"/>  
        /// before calling this.
        /// </summary>
        /// <param name="x">X coordinate of the calibration point</param>
        /// <param name="y">Y coordinate of the calibration point</param>
        public void CalibrationPointStart(int x, int y)
        {
            if (IsActivated)
            {
                if (IsCalibrating)
                    apiManager.RequestCalibrationPointStart(x, y);
                else
                    Debug.WriteLine("TET C# Client calibration not started!");
            }
            else
                Debug.WriteLine("TET C# Client not activated!");
        }

        /// <summary>
        /// Called for every calibration point during a calibration process. This should be
        /// called 1-2 seconds after <see cref="TETCSharpClient.GazeManager.CalibrationPointStart(int,int)"/> .
        /// The calibration process must be initiated by a call to <see cref="CalibrationStart(short, ICalibrationProcessHandler)"/> 
        /// before calling this.
        /// </summary>
        public void CalibrationPointEnd()
        {
            if (IsActivated)
            {
                if (IsCalibrating)
                    apiManager.RequestCalibrationPointEnd();
                else
                    Debug.WriteLine("TET C# Client calibration not started!");
            }
            else
                Debug.WriteLine("TET C# Client not activated!");
        }

        /// <summary>
        /// Cancels an ongoing calibration process.
        /// </summary> 
        public void CalibrationAbort()
        {
            if (IsActivated)
            {
                if (IsCalibrating)
                    apiManager.RequestCalibrationAbort();
                else
                    Debug.WriteLine("TET C# Client calibration not started!");
            }
            else
                Debug.WriteLine("TET C# Client not activated!");
        }

        /// <summary>
        /// Resets calibration state, cancelling any previous calibrations.
        /// </summary>
        public void CalibrationClear()
        {
            if (IsActivated)
                apiManager.RequestCalibrationClear();
            else
                Debug.WriteLine("TET C# Client not activated!");
        }

        #endregion
    }

    /// <summary>
    /// Callback interface with methods associated to Gaze Tracking.
    /// This interface should be implemented by classes that are to recieve live GazeData stream.
    /// </summary>
    public interface IGazeListener
    {
        /// <summary>
        /// A notification call back indicating that a new GazeData frame is available. 
        /// Implementing classes should update themselves accordingly if needed.
        /// Register for updates through GazeManager.AddGazeListener().
        /// </summary>
        /// <param name="gazeData">Latest GazeData frame processed by Tracker Server</param> 
        void OnGazeUpdate(GazeData gazeData);
    }

    /// <summary>
    /// Callback interface with methods associated to the changes of CalibrationResult.
    /// This interface should be implemented by classes that are to recieve only changes in CalibrationResult
    /// and who are _not_ to perform the calibration process itself.
    /// </summary>
    public interface ICalibrationResultListener
    {
        /// <summary>
        /// A notification call back indicating that state of calibration has changed. 
        /// Implementing classes should update themselves accordingly if needed.
        /// Register for updates through GazeManager.AddCalibrationResultListener().
        /// </summary>
        /// <param name="isCalibrated">is the Tracker Server calibrated?</param>
        /// <param name="calibResult">if calibrated, the currently valid CalibrationResult, otherwise null</param>
        void OnCalibrationChanged(bool isCalibrated, CalibrationResult calibResult);
    }
    /// <summary>
    /// Callback interface with methods associated to the state of the physical Tracker device.
    /// This interface should be implemented by classes that are to receive notifications of 
    /// changes in the state of the Tracker and handle these accordingly. This could be a class
    /// in the 'View' layer telling the user that a Tracker has disconnected.
    /// </summary>
    public interface ITrackerStateListener
    {
        /// <summary>
        /// A notification call back indicating that state of connected Tracker device has changed. 
        /// Use this to detect if a tracker has been connected or disconnected.
        /// Implementing classes should update themselves accordingly if needed.
        /// Register for updates through GazeManager.AddTrackerStateListener().
        /// </summary>
        /// <param name="trackerState">the current state of the physical Tracker device</param>
        void OnTrackerStateChanged(GazeManager.TrackerState trackerState);

        /// <summary>
        /// A notification call back indicating that main screen index has changed. 
        /// This is only relevant for multiscreen setups. Implementing classes should
        /// update themselves accordingly if needed.
        /// Register for updates through GazeManager.AddTrackerStateListener().
        /// </summary>
        /// <param name="screenIndex">the currently valid screen index</param>
        /// <param name="screenResolutionWidth">screen resolution width in pixels</param>
        /// <param name="screenResolutionHeight">screen resolution height in pixels</param>
        /// <param name="screenPhysicalWidth">Physical screen width in meters</param>
        /// <param name="screenPhysicalHeight">Physical screen height in meters</param>
        void OnScreenStatesChanged(int screenIndex, int screenResolutionWidth, int screenResolutionHeight, float screenPhysicalWidth, float screenPhysicalHeight);
    }

    /// <summary>
    /// Callback interface with methods associated to Calibration process.
    /// </summary>
    public interface ICalibrationProcessHandler
    {
        /// <summary>
        /// Called when a calibration process has been started. 
        /// </summary>
        void OnCalibrationStarted();

        /// <summary>
        /// Called every time tracking of a single calibration points has completed.
        /// </summary>
        /// <param name="progress">'normalized' progress [0..1d]</param>
        void OnCalibrationProgress(double progress);

        /// <summary>
        /// Called when all calibration points have been collected and calibration processing begins.
        /// </summary>
        void OnCalibrationProcessing();

        /// <summary>
        /// Called when processing of calibration points and calibration as a whole has completed.
        /// </summary>
        /// <param name="calibResult">the results of the calibration process</param>
        void OnCalibrationResult(CalibrationResult calibResult);
    }

    /// <summary>
    /// Callback interface with methods associated to the state of the connection to the
    /// EyeTribe Server. This interface should be implemented by classes that are to
    /// receive notifications of changes in the connection state and handle these
    /// accordingly. This could be a class in the 'View' layer telling the user that the
    /// connection to the EyeTribe Server was lost.
    /// </summary>
    public interface IConnectionStateListener
    {
        /// <summary>
        /// A notification call back indicating that the connection state has changed.
        /// Use this to detect if connection the EyeTribe Server has been lost.
        /// Implementing classes should update themselves accordingly if needed.
        /// Register for updates through GazeManager.AddConnectionStateListener().
        /// </summary>
        /// <param name="isConnected">the current state of the connection</param>
        void OnConnectionStateChanged(bool isConnected);
    }
}