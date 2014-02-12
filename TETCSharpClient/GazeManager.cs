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

namespace TETCSharpClient
{
    /// <summary>
    /// This singleton is the main entry point of the TET C# Client. It manages all routines associated to gaze control.
    /// Using this class a developer can 'calibrate' an eye tracking setup and attach listeners to recieve live data streams
    /// of {@link TETCSharpClient.Data.GazeData} updates.
    /// </summary>
    public class GazeManager : IGazeApiReponseListener
    {
        #region Constants

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

        protected Object initializationLock;

        internal Heartbeat heartbeatHandler;
        private GazeBroadcaster gazeBroadcaster;
        private FixedSizeQueue<GazeData> queueGazeData;

        protected int sampledCalibrationPoints;
        protected int totalCalibrationPoints;

        protected List<IGazeListener> gazeListeners;
        protected List<ICalibrationResultListener> calibrationStateListeners;
        protected List<ITrackerStateListener> trackerStateListeners;
        protected ICalibrationProcessHandler calibrationListener;

        #endregion

        #region Constructor

        private GazeManager()
        {
            gazeListeners = new List<IGazeListener>();
            calibrationStateListeners = new List<ICalibrationResultListener>();
            trackerStateListeners = new List<ITrackerStateListener>();
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
        public bool IsConnected
        {
            get { return (null != apiManager ? apiManager.IsConnected() : false); }
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
        [DefaultValue(3000)]
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

        public void OnGazeApiResponse(String response)
        {
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

                            JObject values = json[Protocol.KEY_VALUES].ToObject<JObject>();
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
                                    if ((int)value != (int)Trackerstate.GetTypeCode())
                                    {
                                        Trackerstate = (TrackerState)(int)value;

                                        lock (((ICollection)trackerStateListeners).SyncRoot)
                                        {
                                            foreach (ITrackerStateListener listener in trackerStateListeners)
                                            {
                                                try
                                                {
                                                    listener.OnTrackerStateChanged(Trackerstate);
                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.WriteLine("Exception while calling ITrackerStateListener.OnTrackerStateChanged() on listener " + listener + ": " + e.StackTrace);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (values.TryGetValue(Protocol.TRACKER_CALIBRATIONRESULT, out value))
                                    LastCalibrationResult = value.ToObject<CalibrationResult>();

                                if (values.TryGetValue(Protocol.TRACKER_ISCALIBRATING, out value))
                                    IsCalibrating = (bool)value;

                                if (values.TryGetValue(Protocol.TRACKER_ISCALIBRATED, out value))
                                {
                                    //if calibration state changed, notify listeners
                                    if ((bool)value != IsCalibrated)
                                    {
                                        IsCalibrated = (bool)value;

                                        lock (((ICollection)calibrationStateListeners).SyncRoot)
                                        {
                                            foreach (ICalibrationResultListener listener in calibrationStateListeners)
                                            {
                                                try
                                                {
                                                    listener.OnCalibrationChanged(IsCalibrated, LastCalibrationResult);
                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.WriteLine("Exception while calling ICalibrationResultListener.OnCalibrationChanged() on listener " + listener + ": " + e.StackTrace);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (values.TryGetValue(Protocol.TRACKER_SCREEN_RESOLUTION_WIDTH, out value))
                                    ScreenResolutionWidth = (int)value;

                                if (values.TryGetValue(Protocol.TRACKER_SCREEN_RESOLUTION_HEIGHT, out value))
                                    ScreenResolutionHeight = (int)value;

                                if (values.TryGetValue(Protocol.TRACKER_SCREEN_PHYSICAL_WIDTH, out value))
                                    ScreenPhysicalWidth = (int)value;

                                if (values.TryGetValue(Protocol.TRACKER_SCREEN_PHYSICAL_HEIGHT, out value))
                                    ScreenPhysicalHeight = (int)value;

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
                                                try
                                                {
                                                    listener.OnScreenStatesChanged(ScreenIndex, ScreenResolutionWidth, ScreenResolutionHeight, ScreenPhysicalWidth, ScreenPhysicalHeight);
                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.WriteLine("Exception while calling ITrackerStateListener.OnScreenStatesChanged() on listener " + listener + ": " + e.StackTrace);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (values.TryGetValue(Protocol.TRACKER_FRAME, out value) && null != gazeBroadcaster)
                                {
                                    //Add gaze update to high frequency broadcasting queue
                                    lock (((ICollection)queueGazeData).SyncRoot)
                                    {
                                        queueGazeData.Enqueue(value.ToObject<GazeData>());
                                    }

                                    events.GetUpdateHandle().Set();
                                }
                            }

                            //Handle initialization
                            if (initializationLock != null)
                            {
                                lock (initializationLock)
                                {
                                    Monitor.Pulse(initializationLock);
                                    initializationLock = null;
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

                                //if calibration state changed, notify listeners
                                if (cper.Values.CalibrationResult.Result != IsCalibrated)
                                {
                                    lock (((ICollection)calibrationStateListeners).SyncRoot)
                                    {
                                        foreach (ICalibrationResultListener listener in calibrationStateListeners)
                                        {
                                            try
                                            {
                                                listener.OnCalibrationChanged(cper.Values.CalibrationResult.Result, cper.Values.CalibrationResult);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception while calling ICalibrationStateListener.OnCalibrationChanged() on listener " + listener + ": " + e.StackTrace);
                                            }
                                        }
                                    }
                                }

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

        /// <summary>
        /// Activates TET C# Client and all underlying routines using default values. Should be called _only_ 
        /// once when an application starts up. Calling thread will be locked during
        /// initialization.
        /// </summary>
        /// <param name="apiVersion"/>Version number of the Tracker API that this client will be compliant to</param>
        /// <param name="mode"/>Mode though which the client will receive GazeData. Either ClientMode.Push or ClientMode.Pull</param>
        /// <returns>True is succesfully activated, false otherwise</returns>
        public bool Activate(ApiVersion apiVersion, ClientMode mode)
        {
            return Activate(apiVersion, mode, GazeApiManager.DEFAULT_SERVER_HOST, GazeApiManager.DEFAULT_SERVER_PORT);
        }

        /// <summary>
        /// Activates TET C# Client and all underlying routines. Should be called _only_ 
        /// once when an application starts up. Calling thread will be locked during
        /// initialization.
        /// </summary>
        /// <param name="apiVersion"/>Version number of the Tracker API that this client will be compliant to</param>
        /// <param name="mode"/>Mode though which the client will receive GazeData. Either ClientMode.Push or ClientMode.Pull</param>
        /// <param name="hostname"/>The host name or IP address where the eye tracking server is running.</param>
        /// <param name="portnumber"/>The port number used for the eye tracking server</param>
        /// <returns>True is succesfully activated, false otherwise</returns>
        public bool Activate(ApiVersion apiVersion, ClientMode mode, string hostname, int portnumber)
        {
            //if already running, deactivate before starting anew
            if (isActive)
                Deactivate();

            //lock calling thread while initializing
            initializationLock = Thread.CurrentThread;
            lock (initializationLock)
            {
                apiManager = new GazeApiManager(this);
                apiManager.Connect(hostname, portnumber);

                if (apiManager.IsConnected())
                {
                    apiManager.RequestTracker(mode, apiVersion);
                    apiManager.RequestAllStates();

                    //We wait untill above requests have been handled by server or timeout occours
                    bool waitSuccess = Monitor.Wait(initializationLock, TimeSpan.FromSeconds(20));

                    if (waitSuccess == false)
                    {
                        Debug.WriteLine("Error initializing GazeManager");
                        return false;
                    }

                    //init heartbeat
                    heartbeatHandler = new Heartbeat(apiManager);
                    heartbeatHandler.Start();

                    isActive = true;
                }
                else
                    Debug.WriteLine("Error initializing GazeManager");

                return isActive;
            }
        }

        /// <summary>
        /// Deactivates TET C# Client and all under lying routines. Should be called when
        /// a application closes down.
        /// </summary>
        public void Deactivate()
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
        }

        /// <summary>
        /// Adds a {@link TETCSharpClient.IGazeListener} to the TET C# client. This listener 
        /// will recieve {@link TETCSharpClient.Data.GazeData} updates when available
        /// </summary>
        /// <param name="listener"/>The {@link TETCSharpClient.IGazeListener} instance to add</param>
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
        /// Remove a {@link TETCSharpClient.IGazeListener} from the TET C# client.
        /// </summary>
        /// <returns>True if succesfully removed, false otherwise</returns>
        /// <param name="listener"/>The {@link TETCSharpClient.IGazeListener} instance to remove</param>
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
        /// Gets current number of attached {@link TETCSharpClient.IGazeListener} instances.
        /// </summary>
        /// <returns>Curent number of listeners</returns>
        public int GetNumGazeListeners()
        {
            if (null != gazeListeners)
                return gazeListeners.Count;

            return -1;
        }

        /// <summary>
        /// Checkes if a given instance of {@link TETCSharpClient.IGazeListener} is currently attached.
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
        /// Adds a {@link TETCSharpClient.ICalibrationStateListener} to the TET C# client. This listener 
        /// will recieve updates about calibration state changes.
        /// </summary>
        /// <param name="listener"/>The {@link TETCSharpClient.ICalibrationStateListener} instance to add</param>
        public void AddCalibrationStateListener(ICalibrationResultListener listener)
        {
            if (null != listener)
            {
                lock (((ICollection)calibrationStateListeners).SyncRoot)
                {
                    if (!calibrationStateListeners.Contains(listener))
                        calibrationStateListeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// Remove a {@link TETCSharpClient.ICalibrationStateListener} from the TET C# client.
        /// </summary>
        /// <returns>True if succesfully removed, false otherwise</returns>
        /// <param name="listener"/>The {@link TETCSharpClient.ICalibrationStateListener} instance to remove</param>
        public bool RemoveCalibrationStateListener(ICalibrationResultListener listener)
        {
            bool result = false;

            lock (((ICollection)calibrationStateListeners).SyncRoot)
            {
                if (calibrationStateListeners.Contains(listener))
                    result = calibrationStateListeners.Remove(listener);
            }

            return result;
        }

        /// <summary>
        /// Gets current number of attached {@link TETCSharpClient.ICalibrationStateListener} instances.
        /// </summary>
        /// <returns>Curent number of listeners</returns>
        public int GetNumCalibrationStateListeners()
        {
            if (null != calibrationStateListeners)
                return calibrationStateListeners.Count;

            return -1;
        }

        /// <summary>
        /// Checkes if a given instance of {@link TETCSharpClient.ICalibrationStateListener} is currently attached.
        /// </summary>
        /// <returns>True if already attached, false otherwise</returns>
        public bool HasCalibrationStateListener(ICalibrationResultListener listener)
        {
            bool result = false;

            lock (((ICollection)calibrationStateListeners).SyncRoot)
            {
                result = calibrationStateListeners.Contains(listener);
            }

            return result;
        }

        /// <summary>
        /// Adds a {@link TETCSharpClient.ITrackerStateListener} to the TET C# client. This listener 
        /// will recieve updates about change of active screen index.
        /// </summary>
        /// <param name="listener"/>The {@link TETCSharpClient.ITrackerStateListener} instance to add</param>
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
        /// Remove a {@link TETCSharpClient.ITrackerStateListener} from the TET C# client.
        /// </summary>
        /// <returns>True if succesfully removed, false otherwise</returns>
        /// <param name="listener"/>The {@link TETCSharpClient.ITrackerStateListener} instance to remove</param>
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
        /// Gets current number of attached {@link TETCSharpClient.ITrackerStateListener} instances.
        /// </summary>
        /// <returns>Curent number of listeners</returns>
        public int GetNumTrackerStateListeners()
        {
            if (null != trackerStateListeners)
                return trackerStateListeners.Count;

            return -1;
        }

        /// <summary>
        /// Checkes if a given instance of {@link TETCSharpClient.ITrackerStateListener} is currently attached.
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
        /// Clear all attached listeners, clears GazeData queue and stop broadcating
        /// </summary>
        public void ClearListeners()
        {
            lock (((ICollection)gazeListeners).SyncRoot)
            {
                gazeListeners.Clear();
            }

            lock (((ICollection)calibrationStateListeners).SyncRoot)
            {
                calibrationStateListeners.Clear();
            }

            lock (((ICollection)trackerStateListeners).SyncRoot)
            {
                trackerStateListeners.Clear();
            }

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
        /// <param name="screenIndex"/>Index of nex screen. On windows 'Primary Screen' has index 0.</param>
        /// <param name="screenResW"/>Screen resolution width in pixels</param>
        /// <param name="screenResH"/>Screen resolution height in pixels</param>
        /// <param name="screenPsyW"/>Physical Screen width in meters</param>
        /// <param name="screenPsyH"/>Physical Screen height in meters</param>
        public void SwitchScreen(int screenIndex, int screenResW, int screenResH, int screenPsyW, int screenPsyH)
        {
            if (isActive)
            {
                apiManager.RequestScreenSwitch(screenIndex, screenResW, screenResH, screenPsyW, screenPsyH);
            }
            else
                Debug.WriteLine("TET C# Client not activated!");
        }

        /// <summary>
        /// Initiate a new calibration process. Must be called before any call to {@link TETCSharpClient.GazeManager.CalibrationPointStart()}
        /// or {@link TETCSharpClient.GazeManager.CalibrationPointEnd()}.
        /// <p>
        /// Any previous (and possible running) calibration process must be completed or aborted before calling this.
        /// <p>
        /// A full calibration process consists of a number of calls to {@link TETCSharpClient.GazeManager.CalibrationPointStart()} 
        /// and {@link TETCSharpClient.GazeManager.CalibrationPointEnd()} matching the total number of clibration points set by the
        /// numCalibrationPoints parameter.
        /// </summary>
        /// <param name="numCalibrationPoints"/>The number of calibration points that will be used in this calibration</param>
        /// <param name="listener"/>The {@link TETCSharpClient.GazeCalibrationListener} instance that will receive callbacks during the 
        /// calibration process</param>
        public void CalibrationStart(short numCalibrationPoints, ICalibrationProcessHandler listener)
        {
            if (isActive)
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
        /// {@link TETCSharpClient.GazeManager.CalibrationPointEnd()} 1-2 seconds later.
        /// <p>
        /// The calibration process must be initiated by a call to {@link TETCSharpClient.GazeManager.CalibrationStart()} 
        /// before calling this.
        /// </summary>
        /// <param name="x"/>X coordinate of the calibration point</param>
        /// <param name="y"/>Y coordinate of the calibration point</param>
        public void CalibrationPointStart(int x, int y)
        {
            if (isActive)
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
        /// called 1-2 seconds after {@link TETCSharpClient.GazeManager.CalibrationPointStart()}.
        /// <p>
        /// The calibration process must be initiated by a call to {@link TETCSharpClient.GazeManager.CalibrationStart()} 
        /// before calling this.
        /// </summary>
        public void CalibrationPointEnd()
        {
            if (isActive)
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
            if (isActive)
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
            if (isActive)
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
        /// <param name="gazeData"/>Latest GazeData frame processed by Tracker Server</param> 
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
        /// Register for updates through GazeManager.AddCalibrationStateListener().
        /// </summary>
        /// <param name="isCalibrated"/>is the Tracker Server calibrated?</param>
        /// <param name="calibResult"/>if calibrated, the currently valid CalibrationResult, otherwise null</param>
        void OnCalibrationChanged(bool isCalibrated, CalibrationResult calibResult);
    }
    /// <summary>
    /// Callback interface with methods associated to the state of the physical Tracker device.
    /// This interface should be implemented by classes that are to recieve changes if the state of Tracker
    /// and handle these accordingly. This could be a class in the 'View' layer telling the user that a 
    /// Tracker has disconnected.
    /// </summary>
    public interface ITrackerStateListener
    {
        /// <summary>
        /// A notification call back indicating that state of connected Tracker device has changed. 
        /// Use this to detect if a tracker has been connected or disconnected.
        /// Implementing classes should update themselves accordingly if needed.
        /// Register for updates through GazeManager.AddTrackerStateListener().
        /// </summary>
        /// <param name="trackerState"/>the current state of the physical Tracker device</param>
        void OnTrackerStateChanged(GazeManager.TrackerState trackerState);

        /// <summary>
        /// A notification call back indicating that main screen index has changed. 
        /// This is only relevant for multiscreen setups. Implementing classes should
        /// update themselves accordingly if needed.
        /// Register for updates through GazeManager.AddTrackerStateListener().
        /// </summary>
        /// <param name="screenIndex"/>the currently valid screen index</param>
        /// <param name="screenResolutionWidth"/>screen resolution width in pixels</param>
        /// <param name="screenResolutionHeight"/>screen resolution height in pixels</param>
        /// <param name="screenPhysicalWidth"/>Physical screen width in meters</param>
        /// <param name="screenPhysicalHeight"/>Physical screen height in meters</param>
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
}