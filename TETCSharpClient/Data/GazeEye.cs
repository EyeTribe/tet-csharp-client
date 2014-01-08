using System;
using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    /// <summary>
    /// Contains tracking results of a single eye.
    /// </summary>
    public class GazeEye
    {
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
        /// Pupil center coordinates in normalized values
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_PUPIL_CENTER)]
        public Point2D PupilCenterCoordinates { get; set; }

        /// <summary>
        /// Pupil size in normalized value
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_PUPIL_SIZE)]
        public double PupilSize { get; set; }

        public GazeEye()
        {
            RawCoordinates = new Point2D();
            SmoothedCoordinates = new Point2D();
            PupilCenterCoordinates = new Point2D();
            PupilSize = 0d;
        }

        public GazeEye(GazeEye other)
        {
            if (null != other)
            {
                RawCoordinates = other.RawCoordinates.Clone();
                SmoothedCoordinates = other.SmoothedCoordinates.Clone();
                PupilCenterCoordinates = other.PupilCenterCoordinates.Clone();
                PupilSize = other.PupilSize;
            }
        }

        public GazeEye Clone()
        {
            return new GazeEye(this);
        }

        public override bool Equals(Object o)
        {
            var other = o as GazeEye;
            if (other != null)
            {
                return
                    RawCoordinates.Equals(other.RawCoordinates) &&
                    SmoothedCoordinates.Equals(other.SmoothedCoordinates) &&
                    PupilCenterCoordinates.Equals(other.PupilCenterCoordinates) &&
                    PupilSize == other.PupilSize;
            }

            return false;
        }
    }
}