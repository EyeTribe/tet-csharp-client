using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using TETCSharpClient.Data;
using TETCSharpClient.Reply;
using System.Diagnostics;
using System.ComponentModel;

namespace TETCSharpClient
{
    /// <summary>
    /// This singleton is the main entry point for the TET C# Client. It manages all routines associated to gaze control.
    /// Using this class a developer can 'calibrate' an eye tracking setup and attach listeners to recieve {@link TETCSharpClient.Data.GazeData} updates.
    /// </summary>
    public class GazeManager : IGazeApiReponseListener
    {
        #region Constants

        public const int FRAME_QUEUE_SIZE = 10;

        #endregion

        #region Enums

        public enum ClientMode
        {
            Push = 1001,
            Pull = 1002
        }

        #endregion

        #region Variables

        private static GazeManager instance;
        internal GazeApiManager apiManager;
        private WaitHandleWrap events;

        protected bool isActive;

        protected Object initializationLock;

        protected int version;
        protected bool push;

        internal Heartbeat heartbeatHandler;
        private GazeBroadcaster gazeBroadcaster;
        private FixedSizeQueue<GazeData> queueGazeData;

        protected int sampledCalibrationPoints;
        protected int totalCalibrationPoints;


        protected List<IGazeUpdateListener> gazeListeners;
        protected IGazeCalibrationListener calibrationListener;

        #endregion

        #region Constructor

        private GazeManager()
        {
            gazeListeners = new List<IGazeUpdateListener>();
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
                            var tgr = JsonConvert.DeserializeObject<TrackerGetReply>(response);

                            if (tgr.Values.Version.HasValue)
                                version = tgr.Values.Version.Value;

                            if (tgr.Values.Push.HasValue)
                                push = tgr.Values.Push.Value;

                            if (tgr.Values.HeartbeatInterval.HasValue)
                                HeartbeatMillis = tgr.Values.HeartbeatInterval.Value;

                            if (tgr.Values.IsCalibrating.HasValue)
                                IsCalibrating = tgr.Values.IsCalibrating.Value;

                            if (tgr.Values.IsCalibrated.HasValue)
                            {
                                //if calibration state changed, notify listeners
                                if (tgr.Values.IsCalibrated.Value != IsCalibrated)
                                {
                                    IsCalibrated = tgr.Values.IsCalibrated.Value;

                                    lock (((ICollection)gazeListeners).SyncRoot)
                                    {
                                        foreach (IGazeUpdateListener listener in gazeListeners)
                                        {
                                            try
                                            {
                                                listener.OnCalibrationStateChanged(IsCalibrated);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception while calling IGazeUpdateListener.OnCalibrationStateChanged() on listener " + listener + ": " + e.StackTrace);
                                            }
                                        }
                                    }
                                }
                            }

                            if (tgr.Values.ScreenResolutionWidth.HasValue)
                                ScreenResolutionWidth = tgr.Values.ScreenResolutionWidth.Value;

                            if (tgr.Values.ScreenResolutionHeight.HasValue)
                                ScreenResolutionHeight = tgr.Values.ScreenResolutionHeight.Value;

                            if (tgr.Values.ScreenPhysicalWidth.HasValue)
                                ScreenPhysicalWidth = tgr.Values.ScreenPhysicalWidth.Value;

                            if (tgr.Values.ScreenResolutionHeight.HasValue)
                                ScreenPhysicalHeight = tgr.Values.ScreenResolutionHeight.Value;

                            if (tgr.Values.ScreenIndex.HasValue)
                            {
                                //if screen index changed, notify listeners
                                if (tgr.Values.ScreenIndex.Value != ScreenIndex)
                                {
                                    ScreenIndex = tgr.Values.ScreenIndex.Value;

                                    lock (((ICollection)gazeListeners).SyncRoot)
                                    {
                                        foreach (IGazeUpdateListener listener in gazeListeners)
                                        {
                                            try
                                            {
                                                listener.OnScreenIndexChanged(ScreenIndex);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception while calling IGazeUpdateListener.OnScreenIndexChanged() on listener " + listener + ": " + e.StackTrace);
                                            }
                                        }
                                    }
                                }
                            }

                            if (null != tgr.Values.Frame && null != gazeBroadcaster)
                            {
                                //Add gaze update to high frequency broadcasting queue
                                lock (((ICollection)queueGazeData).SyncRoot)
                                {
                                    queueGazeData.Enqueue(tgr.Values.Frame);
                                }

                                events.GetUpdateHandle().Set();
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
                        else
                            if (reply.Request.Equals(Protocol.TRACKER_REQUEST_SET))
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
                                        Debug.WriteLine("Exception while calling IGazeCalibrationListener.OnCalibrationStarted() on listener " + calibrationListener + ": " + e.StackTrace);
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
                                        calibrationListener.OnCalibrationProgressUpdate(sampledCalibrationPoints / totalCalibrationPoints);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine("Exception while calling IGazeCalibrationListener.OnCalibrationProgressUpdate() on listener " + calibrationListener + ": " + e.StackTrace);
                                    }


                                    if (sampledCalibrationPoints == totalCalibrationPoints)
                                        //Notify calibration listener that all calibration points have been sampled and the analysis of the calirbation results has begun 
                                        try
                                        {
                                            calibrationListener.OnCalibrationProcessingResults();
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Exception while calling IGazeCalibrationListener.OnCalibrationProcessingResults() on listener " + calibrationListener + ": " + e.StackTrace);
                                        }
                                }

                                var cper = JsonConvert.DeserializeObject<CalibrationPointEndReply>(response);

                                if (cper == null || cper.Values.CalibrationResult == null)
                                    break; // not done with calibration yet

                                IsCalibrated = cper.Values.CalibrationResult.Result;
                                IsCalibrating = !cper.Values.CalibrationResult.Result;

                                if (null != calibrationListener)
                                {
                                    //Notify calibration listener that calibration results are ready for evaluation
                                    try
                                    {
                                        calibrationListener.OnCalibrationResult(cper.Values.CalibrationResult);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine("Exception while calling IGazeCalibrationListener.OnCalibrationResult() on listener " + calibrationListener + ": " + e.StackTrace);
                                    }
                                }
                                break;

                            case Protocol.CALIBRATION_REQUEST_ABORT:
                                IsCalibrating = false;
                                break;

                            case Protocol.CALIBRATION_REQUEST_CLEAR:
                                IsCalibrated = false;
                                IsCalibrating = false;
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
        /// Activates TET C# Client and all underlying routines. Should be called _only_ 
        /// once when an application starts up. Calling thread will be locked during
        /// initialization.
        /// </summary>
        /// <param name="apiVersion"/>Version number of the Tracker API that this client will be compliant to</param>
        /// <param name="mode"/>Mode though which the client will receive GazeData. Either ClientMode.Push or ClientMode.Pull</param>
        /// <returns>True is succesfully activated, false otherwise</returns>
        public bool Activate(int apiVersion, ClientMode mode)
        {
            //if already running, deactivate before starting anew
            if (isActive)
                Deactivate();

            //lock calling thread while initializing
            initializationLock = Thread.CurrentThread;
            lock (initializationLock)
            {
                apiManager = new GazeApiManager(this);
                apiManager.Connect();

                if (apiManager.IsConnected())
                {
                    apiManager.RequestTracker(mode, apiVersion);
                    apiManager.RequestAllStates();

                    //We wait untill above requests have been handled by server
                    bool waitSuccess = Monitor.Wait(initializationLock, TimeSpan.FromSeconds(20));

                    if(waitSuccess == false)
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

            //clearing gazelisteners will stop heartbeat and broadcasting threads
            ClearGazeListeners();

            isActive = false;
        }

        /// <summary>
        /// Adds a {@link TETCSharpClient.GazeUpdateListener} to the TET C# client. This listener 
        /// will recieve {@link TETCSharpClient.Data.GazeData} updates when available
        /// </summary>
        /// <param name="listener"/>The {@link TETCSharpClient.GazeUpdateListener} instance to add</param>
        public void AddGazeListener(IGazeUpdateListener listener)
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
        /// Remove a {@link TETCSharpClient.GazeUpdateListener} from the TET C# client.
        /// </summary>
        /// <returns>True if succesfully removed, false otherwise</returns>
        /// <param name="listener"/>The {@link TETCSharpClient.GazeUpdateListener} instance to remove</param>
        public bool RemoveGazeListener(IGazeUpdateListener listener)
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
        /// Gets current number of attached {@link TETCSharpClient.GazeUpdateListener} instances.
        /// </summary>
        /// <returns>Curent number of listeners</returns>
        public int GetNumGazeListeners()
        {
            if (null != gazeListeners)
                return gazeListeners.Count;

            return -1;
        }

        /// <summary>
        /// Checkes if a given instance of {@link TETCSharpClient.GazeUpdateListener} is currently attached.
        /// </summary>
        /// <returns>True if already attached, false otherwise</returns>
        public bool HasGazeListener(IGazeUpdateListener listener)
        {
            bool result = false;

            lock (((ICollection)gazeListeners).SyncRoot)
            {
                result = gazeListeners.Contains(listener);
            }

            return result;
        }

        /// <summary>
        /// Clear all attached instances of {@link TETCSharpClient.GazeUpdateListener}.
        /// </summary>
        public void ClearGazeListeners()
        {
            lock (((ICollection)gazeListeners).SyncRoot)
            {
                gazeListeners.Clear();
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
        /// <param name="listener"/>The {@link TETCSharpClient.GazeCalibrationListener} instance that will receive callbacks during the calibration process</param>
        public void CalibrationStart(short numCalibrationPoints, IGazeCalibrationListener listener)
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
        /// Called for every calibration point during a calibration process. This should be called 1-2 seconds after {@link TETCSharpClient.GazeManager.CalibrationPointStart()}.
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
        public void CalibrationReset()
        {
            if (isActive)
                apiManager.RequestCalibrationReset();
            else
                Debug.WriteLine("TET C# Client not activated!");
        }

        #endregion
    }

    /// <summary>
    /// Callback interface with methods associated to Gaze Tracking.
    /// </summary>
    public interface IGazeUpdateListener
    {
        /// <summary>
        /// A notification call back indicating that a new GazeData frame is available. 
        /// Listening clients should update themselves accordingly if needed.
        /// </summary>
        void OnGazeUpdate(GazeData gazeData);

        /// <summary>
        /// A notification call back indicating that state of calibration has changed. 
        /// Listening clients should update themselves accordingly if needed.
        /// </summary>
        void OnCalibrationStateChanged(bool isCalibrated);

        /// <summary>
        /// A notification call back indicating that main screen index has changed. 
        /// This is only relevant for multiscreen setups. Listening clients should
        /// update themselves accordingly if needed.
        /// </summary>
        void OnScreenIndexChanged(int screenIndex);
    }

    /// <summary>
    /// Callback interface with methods associated to Gaze Calibration.
    /// </summary>
    public interface IGazeCalibrationListener
    {
        /// <summary>
        /// Called when a calibraiton process has been started. 
        /// </summary>
        void OnCalibrationStarted();

        /// <summary>
        /// Called every time tracking of a single calibratioon points has completed.
        /// </summary>
        /// <param name="progress">'normalized' progress [0..1.0d]</param>
        void OnCalibrationProgressUpdate(double progress);

        /// <summary>
        /// Called when processing of tracked calibration points begin.
        /// </summary>
        void OnCalibrationProcessingResults();

        /// <summary>
        /// Called when processing of calibration points and calibration as a whole has completed.
        /// </summary>
        /// <param name="calibResult">the results of the calibration process</param>
        bool OnCalibrationResult(CalibrationResult calibResult);
    }
}
