using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using EyeTribe.ClientSdk.Utils;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;


namespace EyeTribe.ClientSdk.Test
{
    [TestFixture]
    public class EyeTribeUnitTest
    {
        public const long DEFAULT_TIMEOUT_SECONDS = 5;
        public const long DEFAULT_TIMEOUT_MILLIS = DEFAULT_TIMEOUT_SECONDS * 1000;
        Object SyncRoot = new Object();

        [Test]
        public void TestGazeData()
        {
            GazeData gd = new GazeData();
            gd.LeftEye.PupilSize = 11.1d;
            gd.RightEye.PupilCenterCoordinates = new Point2D(800.13f, 20.325f);
            gd.State = 1;

            Assert.IsNotNull(gd.State);
            Assert.IsTrue(!gd.HasRawGazeCoordinates());

            String json = JsonConvert.SerializeObject(gd);
            GazeData gd2 = JsonConvert.DeserializeObject<GazeData>(json);

            Assert.AreEqual(gd, gd2);
            Assert.AreEqual(gd.GetHashCode(), gd2.GetHashCode());
        }

        [Test]
        public void TestCalibrationResult()
        {
            CalibrationResult cr = new CalibrationResult();
            cr.AverageErrorDegree = 11.1d;
            cr.AverageErrorDegreeRight = 800.13d;

            CalibrationPoint cp = new CalibrationPoint();
            cp.StandardDeviation.Left = 123.123d;
            cp.Accuracy.Left = 324.159d;
            cp.MeanError.Left = 657.159d;
            cp.Coordinates = new Point2D(321.123f, 432.234f);
            cr.Calibpoints = new CalibrationPoint[] { cp };

            String json = JsonConvert.SerializeObject(cr);
            CalibrationResult cr2 = JsonConvert.DeserializeObject<CalibrationResult>(json);

            Assert.AreEqual(cr, cr2);
            Assert.AreEqual(cr.GetHashCode(), cr2.GetHashCode());
        }

        [Test]
        public void TestActivate()
        {
            try
            {
                Boolean activated = GazeManager.Instance.Activate();
                Assert.IsTrue(activated);
                Assert.IsTrue(GazeManager.Instance.IsActivated);
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestActivateAsync()
        {
            try
            {
                Task<Boolean> task = GazeManager.Instance.ActivateAsync();
                Assert.IsNotNull(task);
                Assert.IsTrue(task.Result);
                Assert.IsTrue(GazeManager.Instance.IsActivated);
            }
            finally
            {
                DeactivateServer();
            }
        }


        [Test]
        public void TestSwitchScreen()
        {
            try
            {
                ActivateServer();

                Assert.IsFalse(GazeManager.Instance.SwitchScreen(5, 1980, 1200, .4f, .3f));
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestSwitchScreenAsync()
        {
            try
            {
                ActivateServer();

                Task<Boolean> task = GazeManager.Instance.SwitchScreenAsync(5, 1980, 1200, .4f, .3f);
                Assert.IsNotNull(task);
                Assert.IsFalse(task.Result);
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestCalibrationStart()
        {
            try
            {
                ActivateServer();

                TestListener listener = new TestListener();

                GazeManager.Instance.CalibrationAbort();

                GazeManager.Instance.AddCalibrationStateListener(listener);

                Assert.IsTrue(GazeManager.Instance.CalibrationStart(9, null));

                Assert.IsFalse(GazeManager.Instance.CalibrationStart(9, null));

                lock (SyncRoot)
                {
                    Monitor.Wait(SyncRoot, 1000);
                }

                Assert.IsTrue(listener.hasRecievedCalibrationStateChange);

                Assert.IsTrue(GazeManager.Instance.CalibrationAbort());

                Assert.IsTrue(GazeManager.Instance.CalibrationStart(9, null));

                Assert.IsTrue(GazeManager.Instance.CalibrationAbort());
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestCalibrationStartAsync()
        {
            try
            {
                ActivateServer();

                TestListener listener = new TestListener();

                GazeManager.Instance.CalibrationAbort();

                GazeManager.Instance.AddCalibrationStateListener(listener);

                Task<Boolean> task = GazeManager.Instance.CalibrationStartAsync(9, null);
                Assert.IsNotNull(task);
                Assert.IsTrue(task.Result);

                task = GazeManager.Instance.CalibrationStartAsync(9, null);
                Assert.IsNotNull(task);
                Assert.IsFalse(task.Result);

                lock (SyncRoot)
                {
                    Monitor.Wait(SyncRoot, 1000);
                }

                Assert.IsTrue(listener.hasRecievedCalibrationStateChange);

                Assert.IsTrue(GazeManager.Instance.CalibrationAbort());

                task = GazeManager.Instance.CalibrationStartAsync(9, null);
                Assert.IsNotNull(task);
                Assert.IsTrue(task.Result);

                Assert.IsTrue(GazeManager.Instance.CalibrationAbort());
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestGazeDataStream()
        {
            try
            {
                ActivateServer();

                TestListener listener = new TestListener();

                Assert.IsFalse(GazeManager.Instance.HasGazeListener(listener));

                GazeManager.Instance.AddGazeListener(listener);

                Assert.IsTrue(GazeManager.Instance.HasGazeListener(listener));

                Assert.IsTrue(GazeManager.Instance.GetNumGazeListeners() == 1);

                lock (SyncRoot)
                {
                    Monitor.Wait(SyncRoot, (int)DEFAULT_TIMEOUT_MILLIS);
                }

                Assert.IsTrue(listener.hasRecievedGazeData);
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestRapidActivation()
        {
            try
            {
                for (int i = 20; --i >= 0; )
                {
                    GazeManager.Instance.ActivateAsync();
                    GazeManager.Instance.Deactivate();
                    Assert.IsFalse(GazeManager.Instance.IsActivated);
                }

                Assert.IsFalse(GazeManager.Instance.IsActivated);
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestAsyncLocks()
        {
            try
            {
                for (int i = 20; --i >= 0; )
                {
                    Debug.WriteLine("TestAsyncLocks cycle: " + i);

                    Assert.IsFalse(GazeManager.Instance.IsActivated);
                    Assert.IsTrue(GazeManager.Instance.Activate());
                    Assert.IsTrue(GazeManager.Instance.IsActivated);

                    lock (SyncRoot)
                    {
                        Monitor.Wait(SyncRoot, 1000);
                    }

                    Task<Boolean> task = GazeManager.Instance.CalibrationStartAsync(9, null);
                    Assert.IsTrue(task.Result);

                    task = GazeManager.Instance.CalibrationAbortAsync();
                    Assert.IsTrue(task.Result);

                    lock (SyncRoot)
                    {
                        Monitor.Wait(SyncRoot, 1000);
                    }

                    GazeManager.Instance.Deactivate();

                    lock (SyncRoot)
                    {
                        Monitor.Wait(SyncRoot, 1000);
                    }

                    Assert.IsFalse(GazeManager.Instance.IsActivated);
                }

                Assert.IsFalse(GazeManager.Instance.IsActivated);
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestCalibrationProcessHandler()
        {
            try
            {
                try
                {
                    ActivateServer();

                    CalibrationProcessHandler handler = new CalibrationProcessHandler();

                    List<Point2D> calibPoints = CalibUtils.InitCalibrationPoints(3, 3, 1980, 1200, 30, 30, true);

                    Assert.IsTrue(GazeManager.Instance.CalibrationStart(9, handler));

                    Thread.Sleep(500);

                    foreach (Point2D p in calibPoints)
                    {
                        GazeManager.Instance.CalibrationPointStart((int)p.X, (int)p.Y);

                        Thread.Sleep(500);

                        GazeManager.Instance.CalibrationPointEnd();

                        Thread.Sleep(1000);
                    }

                    Thread.Sleep(2000);

                    Assert.IsTrue(handler.startWasCalled);
                    Assert.IsTrue(handler.progressWasCalled);
                    Assert.IsTrue(handler.processingWasCalled);
                    Assert.IsTrue(handler.resultWasCalled);

                    Assert.IsNotNull(handler.result);

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception testing calibration: " + e.Message);
                }
            }
            finally
            {
                DeactivateServer();
            }
        }

        [Test]
        public void TestListenerRegistrationRaceCondition()
        {
            try
            {
                ActivateServer();

                Object obj = new Object();

                Thread t = new Thread(() =>
                {
                    Thread t1 = new Thread(new ThreadStart(new ThreadTestListener(10 * 1000).Work));
                    t1.Name = "ThreadTest1";
                    t1.Start();

                    Thread t2 = new Thread(new ThreadStart(new ThreadTestListener(10 * 1000).Work));
                    t2.Name = "ThreadTest2";
                    t2.Start();

                    Thread t3 = new Thread(new ThreadStart(new ThreadTestListener(10 * 1000).Work));
                    t3.Name = "ThreadTest3";
                    t3.Start();

                    Thread t4 = new Thread(new ThreadStart(new ThreadTestListener(10 * 1000).Work));
                    t4.Name = "ThreadTest4";
                    t4.Start();

                    Thread.Sleep(12 * 1000);

                    DeactivateServer();

                    lock (obj)
                    {
                        Monitor.Pulse(obj);
                    }
                });
                t.Start();

                lock (obj)
                {
                    Monitor.Wait(obj);
                }

                Assert.IsFalse(GazeManager.Instance.IsActivated);
            }
            finally
            {
                DeactivateServer();
            }
        }

        private void ActivateServer()
        {
            Task<Boolean> task = GazeManager.Instance.ActivateAsync();
            Assert.IsNotNull(task);
            Assert.IsTrue(task.Result);
            Assert.IsTrue(GazeManager.Instance.Trackerstate == GazeManager.TrackerState.TRACKER_CONNECTED);
        }

        private void DeactivateServer()
        {
            GazeManager.Instance.Deactivate();
            lock (SyncRoot)
            {
                Monitor.Wait(SyncRoot, (int)DEFAULT_TIMEOUT_MILLIS);
            }
            Assert.IsFalse(GazeManager.Instance.IsActivated);
        }

        class ThreadTestListener : TestListener
        {
            private bool isRunning = true;
            private Random random = new Random();
            int time;
            long timestamp = -1;
            int num;

            bool ticker;

            public ThreadTestListener(int time)
            {
                this.time = time;
            }

            public void Work()
            {
                while (isRunning)
                {
                    try
                    {
                        if (timestamp < 0)
                            timestamp = (long)((double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

                        long now = (long)((double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                        if (now - timestamp > time)
                        {
                            isRunning = false;
                        }

                        if (!ticker)
                        {
                            GazeManager.Instance.AddGazeListener(this);

                            if (GazeManager.DebugMode)
                                Debug.WriteLine("AddGazeListener: " + GazeManager.Instance.GetNumGazeListeners());

                            GazeManager.Instance.AddConnectionStateListener(this);

                            GazeManager.Instance.AddCalibrationResultListener(this);

                            GazeManager.Instance.AddTrackerStateListener(this);

                            GazeManager.Instance.AddScreenStateListener(this);

                            Assert.IsTrue(GazeManager.Instance.HasGazeListener(this));
                            Assert.IsTrue(GazeManager.Instance.HasConnectionStateListener(this));
                            Assert.IsTrue(GazeManager.Instance.HasCalibrationResultListener(this));
                            Assert.IsTrue(GazeManager.Instance.HasTrackerStateListener(this));
                            Assert.IsTrue(GazeManager.Instance.HasScreenStateListener(this));
                        }
                        else
                        {
                            GazeManager.Instance.RemoveGazeListener(this);

                            if (GazeManager.DebugMode)
                                Debug.WriteLine("RemoveGazeListener: " + GazeManager.Instance.GetNumGazeListeners());

                            GazeManager.Instance.RemoveConnectionStateListener(this);

                            GazeManager.Instance.RemoveCalibrationResultListener(this);

                            GazeManager.Instance.RemoveTrackerStateListener(this);

                            GazeManager.Instance.RemoveScreenStateListener(this);

                            Assert.IsFalse(GazeManager.Instance.HasGazeListener(this));
                            Assert.IsFalse(GazeManager.Instance.HasConnectionStateListener(this));
                            Assert.IsFalse(GazeManager.Instance.HasCalibrationResultListener(this));
                            Assert.IsFalse(GazeManager.Instance.HasTrackerStateListener(this));
                            Assert.IsFalse(GazeManager.Instance.HasScreenStateListener(this));
                        }

                        ticker = !ticker;

                        Thread.Sleep(10 + (random.Next() % 250));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("e: " + e.Message);
                    }
                }
            }
        }

        class TestListener : IGazeListener, IConnectionStateListener, ICalibrationResultListener, ITrackerStateListener,
            IScreenStateListener, ICalibrationStateListener
        {
            public bool hasRecievedConnecitonStateChange;
            public bool hasRecievedTrackerStateChange;
            public bool hasRecievedScreenStateChange;
            public bool hasRecievedCalibrationResultChange;
            public bool hasRecievedCalibrationStateChange;
            public bool hasRecievedGazeData;

            public void OnConnectionStateChanged(Boolean isConnected)
            {
                if (!hasRecievedConnecitonStateChange)
                    hasRecievedConnecitonStateChange = !hasRecievedConnecitonStateChange;

                if (GazeManager.DebugMode)
                    Debug.WriteLine("Thread: " + Thread.CurrentThread.Name + ", onConnectionStateChanged: " + isConnected);
            }

            public void OnTrackerStateChanged(GazeManager.TrackerState trackerState)
            {
                if (!hasRecievedTrackerStateChange)
                    hasRecievedTrackerStateChange = !hasRecievedTrackerStateChange;

                if (GazeManager.DebugMode)
                    Debug.WriteLine("Thread: " + Thread.CurrentThread.Name + ", onTrackerStateChanged: " + trackerState);
            }

            public void OnScreenStatesChanged(int screenIndex,
                    int screenResolutionWidth, int screenResolutionHeight,
                    float screenPhysicalWidth, float screenPhysicalHeight)
            {
                if (!hasRecievedScreenStateChange)
                    hasRecievedScreenStateChange = !hasRecievedScreenStateChange;

                if (GazeManager.DebugMode)
                    Debug.WriteLine("Thread: " + Thread.CurrentThread.Name + ", OnScreenStatesChanged: " + screenIndex + ", " + screenResolutionWidth + ", " + screenResolutionHeight + ", " + screenPhysicalWidth + ", " + screenPhysicalHeight);
            }

            public void OnCalibrationChanged(Boolean isCalibrated,
                    CalibrationResult calibResult)
            {
                if (!hasRecievedCalibrationResultChange)
                    hasRecievedCalibrationResultChange = !hasRecievedCalibrationResultChange;

                if (GazeManager.DebugMode)
                    Debug.WriteLine("Thread: " + Thread.CurrentThread.Name + ", onCalibrationChanged: " + isCalibrated + ", " + calibResult.ToString());
            }

            public void OnGazeUpdate(GazeData gazeData)
            {
                if (!hasRecievedGazeData)
                    hasRecievedGazeData = !hasRecievedGazeData;

                if (GazeManager.DebugMode)
                    Debug.WriteLine("Thread: " + Thread.CurrentThread.Name + ", onGazeUpdate: " + gazeData.ToString());
            }

            public void OnCalibrationStateChanged(bool isCalibrating, bool isCalibrated)
            {
                Debug.WriteLine("ROEV hasRecievedCalibrationStateChange: " + hasRecievedCalibrationStateChange);

                if (!hasRecievedCalibrationStateChange)
                    hasRecievedCalibrationStateChange = !hasRecievedCalibrationStateChange;

                Debug.WriteLine("ROEV2 hasRecievedCalibrationStateChange: " + hasRecievedCalibrationStateChange);

                if (GazeManager.DebugMode)
                    Debug.WriteLine("Thread: " + Thread.CurrentThread.Name + ", OnCalibrationStateChanged: " + isCalibrating + ", " + isCalibrated);
            }
        }

        class CalibrationProcessHandler : ICalibrationProcessHandler
        {
            public bool startWasCalled;
            public bool progressWasCalled;
            public bool processingWasCalled;
            public bool resultWasCalled;

            public CalibrationResult result;

            public void OnCalibrationStarted()
            {
                startWasCalled = true;
            }

            public void OnCalibrationResult(CalibrationResult calibResult)
            {
                resultWasCalled = true;
                result = calibResult;
            }

            public void OnCalibrationProgress(double progress)
            {
                progressWasCalled = true;
            }

            public void OnCalibrationProcessing()
            {
                processingWasCalled = true;
            }
        }
    }
}
