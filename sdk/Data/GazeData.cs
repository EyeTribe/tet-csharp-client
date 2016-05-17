/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using Newtonsoft.Json;

namespace EyeTribe.ClientSdk.Data
{
    /// <summary>
    /// Contains eye tracking results of a single frame. It holds a state that defines
    /// the quality of the current tracking and fine grained tracking details down to eye level.
    /// </summary>
    public class GazeData
    {
        #region Constants

        public const string TIMESTAMP_STRING_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

        #endregion

        #region Public properties

        /// <summary>
        /// Set when engine is calibrated and glint tracking successfully.
        /// </summary>
        public const int STATE_TRACKING_GAZE = 1;

        /// <summary>
        /// Set when engine has detected eyes.
        /// </summary>
        public const int STATE_TRACKING_EYES = 1 << 1;

        /// <summary>
        /// Set when engine has detected either face, eyes or glint.
        /// </summary>
        public const int STATE_TRACKING_PRESENCE = 1 << 2;

        /// <summary>
        /// Set when tracking failed in the last process frame.
        /// </summary>
        public const int STATE_TRACKING_FAIL = 1 << 3;

        /// <summary>
        /// Set when tracking has failed consecutively over a period of time defined by engine.
        /// </summary>
        public const int STATE_TRACKING_LOST = 1 << 4;

        /// <summary>
        /// State of this frame. States can be extracted using the STATE_ mask constants.
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_STATE)]
        public int State { get; set; }

        /// <summary>
        /// Raw gaze coordinates in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_RAW_COORDINATES)]
        public Point2D RawCoordinates { get; set; }

        /// <summary>
        /// Smoothed gaze coordinates in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_AVERAGE_COORDINATES)]
        public Point2D SmoothedCoordinates { get; set; }

        /// <summary>
        /// Left GazeEye object
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_LEFT_EYE)]
        public Eye LeftEye { get; set; }

        /// <summary>
        /// Right GazeEye object
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_RIGHT_EYE)]
        public Eye RightEye { get; set; }

        /// <summary>
        /// Timestamp for this frame
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_TIME)]
        public long TimeStamp { get; set; }

        /// <summary>
        /// Timestamp for this frame represented in String form
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_TIMESTAMP)]
        public String TimeStampString { get; set; }

        /// <summary>
        /// Is user fixated in this frame?
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_FIXATION)]
        public bool IsFixated { get; set; }

        #endregion

        #region Constructor

        public GazeData()
        {
            DateTime now = DateTime.Now;
            TimeStamp = (long)((double)now.Ticks / TimeSpan.TicksPerMillisecond);
            TimeStampString = now.ToString(TIMESTAMP_STRING_FORMAT);
            IsFixated = false;
            RawCoordinates = new Point2D();
            SmoothedCoordinates = new Point2D();

            LeftEye = new Eye();
            RightEye = new Eye();
        }

        public GazeData(GazeData other)
        {
            if (null != other)
            {
                State = other.State;
                TimeStamp = other.TimeStamp;
                TimeStampString = other.TimeStampString;

                RawCoordinates = new Point2D(other.RawCoordinates);
                SmoothedCoordinates = new Point2D(other.SmoothedCoordinates);

                LeftEye = new Eye(other.LeftEye);
                RightEye = new Eye(other.RightEye);

                IsFixated = other.IsFixated;
            }
        }

        #endregion

        #region Public methods

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is GazeData))
                return false;

            var other = o as GazeData;

            return
                this.RawCoordinates.Equals(other.RawCoordinates) &&
                this.SmoothedCoordinates.Equals(other.SmoothedCoordinates) &&
                this.LeftEye.Equals(other.LeftEye) &&
                this.RightEye.Equals(other.RightEye) &&
                this.State == other.State &&
                this.IsFixated == other.IsFixated;
        }

        public override int GetHashCode()
        {
            int hash = 2039;
            hash = hash * 1553 + RawCoordinates.GetHashCode();
            hash = hash * 1553 + SmoothedCoordinates.GetHashCode();
            hash = hash * 1553 + LeftEye.GetHashCode();
            hash = hash * 1553 + RightEye.GetHashCode();
            hash = hash * 1553 + State.GetHashCode();
            hash = hash * 1553 + IsFixated.GetHashCode();
            return hash;
        }

        public String StateToString()
        {
            String stateString = "";
            bool ticker = false;

            if ((STATE_TRACKING_GAZE & State) != 0)
            {
                stateString += "STATE_TRACKING_GAZE";
                ticker = true;
            }

            if ((STATE_TRACKING_GAZE & State) != 0)
            {
                stateString += (ticker ? " | " : "") + "STATE_TRACKING_EYES";
                ticker = true;
            }

            if ((STATE_TRACKING_PRESENCE & State) != 0)
            {
                stateString += (ticker ? " | " : "") + "STATE_TRACKING_PRESENCE";
                ticker = true;
            }

            if ((STATE_TRACKING_FAIL & State) != 0)
            {
                stateString += (ticker ? " | " : "") + "STATE_TRACKING_FAIL";
                ticker = true;
            }

            if ((STATE_TRACKING_LOST & State) != 0)
            {
                stateString += (ticker ? " | " : "") + "STATE_TRACKING_LOST";
                ticker = true;
            }

            return stateString;
        }

        private const int NO_TRACKING_MASK = STATE_TRACKING_LOST | STATE_TRACKING_FAIL;

        public Boolean HasSmoothedGazeCoordinates()
        {
            return (State & NO_TRACKING_MASK) == 0 && SmoothedCoordinates.X != 0 && SmoothedCoordinates.Y != 0;
        }

        public Boolean HasRawGazeCoordinates()
        {
            return (State & NO_TRACKING_MASK) == 0 && RawCoordinates.X != 0 && RawCoordinates.Y != 0;
        }

        #endregion

        #region Private methods

        private void Set(GazeData other)
        {
            State = other.State;
            TimeStamp = other.TimeStamp;
            TimeStampString = other.TimeStampString;

            RawCoordinates = new Point2D(other.RawCoordinates);
            SmoothedCoordinates = new Point2D(other.SmoothedCoordinates);

            LeftEye = new Eye(other.LeftEye);
            RightEye = new Eye(other.RightEye);

            IsFixated = other.IsFixated;
        }

        #endregion
    }
}