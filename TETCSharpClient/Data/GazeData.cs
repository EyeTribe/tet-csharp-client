using System;
using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    /// <summary>
    /// Contains eye tracking results of a single frame. It holds a state that defines
    /// the quality of the current tracking and fine grained tracking details down to eye level.
    /// </summary>
    public class GazeData
    {
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
            TimeStamp = (long)Math.Round(DateTime.Now.TimeOfDay.TotalMilliseconds);
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

                RawCoordinates = other.RawCoordinates.Clone();
                SmoothedCoordinates = other.SmoothedCoordinates.Clone();

                LeftEye = other.LeftEye.Clone();
                RightEye = other.RightEye.Clone();

                IsFixated = other.IsFixated;
            }
        }

        public GazeData(String json)
        {
            Set(JsonConvert.DeserializeObject<GazeData>(json));
        }

        #endregion

        #region Public methods

        public GazeData Clone()
        {
            return new GazeData(this);
        }

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is GazeData))
                return false;

            var other = o as GazeData;

            return
                State == other.State &&
                TimeStamp == other.TimeStamp &&
                RawCoordinates.Equals(other.RawCoordinates) &&
                SmoothedCoordinates.Equals(other.SmoothedCoordinates) &&
                LeftEye.Equals(other.LeftEye) &&
                RightEye.Equals(other.RightEye) &&
                IsFixated == other.IsFixated;
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

        #region Private methods

        private void Set(GazeData other)
        {
            State = other.State;
            TimeStamp = other.TimeStamp;

            RawCoordinates = other.RawCoordinates;
            SmoothedCoordinates = other.SmoothedCoordinates;

            LeftEye = other.LeftEye;
            RightEye = other.RightEye;

            IsFixated = other.IsFixated;
        }

        #endregion
    }
}