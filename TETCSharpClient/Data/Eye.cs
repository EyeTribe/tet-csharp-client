/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    /// <summary>
    /// Contains tracking results of a single eye.
    /// </summary>
    public class Eye
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
        [JsonProperty(PropertyName = Protocol.FRAME_EYE_PUPIL_CENTER)]
        public Point2D PupilCenterCoordinates { get; set; }

        /// <summary>
        /// Pupil size in normalized value
        /// </summary>
        [JsonProperty(PropertyName = Protocol.FRAME_EYE_PUPIL_SIZE)]
        public double PupilSize { get; set; }

        public Eye()
        {
            RawCoordinates = new Point2D();
            SmoothedCoordinates = new Point2D();
            PupilCenterCoordinates = new Point2D();
            PupilSize = 0d;
        }

        public Eye(Eye other)
        {
            if (null != other)
            {
                RawCoordinates = new Point2D(other.RawCoordinates);
                SmoothedCoordinates = new Point2D(other.SmoothedCoordinates);
                PupilCenterCoordinates = new Point2D(other.PupilCenterCoordinates);
                PupilSize = other.PupilSize;
            }
        }

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is Eye))
                return false;

            var other = o as Eye;

            return
                RawCoordinates.Equals(other.RawCoordinates) &&
                SmoothedCoordinates.Equals(other.SmoothedCoordinates) &&
                PupilCenterCoordinates.Equals(other.PupilCenterCoordinates) &&
                Double.Equals(PupilSize, other.PupilSize);
        }
    }
}