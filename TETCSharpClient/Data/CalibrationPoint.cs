/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;
using System;

namespace TETCSharpClient.Data
{
    public class CalibrationPoint
    {
        /// <summary>
        /// State defines that no data is available for calibration point
        /// </summary>
        public static readonly int STATE_NO_DATA = 0;

        /// <summary>
        /// State defines that calibration point should be resampled
        /// </summary>
        public static readonly int STATE_RESAMPLE = 1;

        /// <summary>
        /// State defines that calibration point was successfully sampled
        /// </summary>
        public static readonly int STATE_OK = 2;

        /// <summary>
        /// State of calib point
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_STATE)]
        public int State { get; set; }

        /// <summary>
        /// Coordinate in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_COORDINATES)]
        public Point2D Coordinates { get; set; }

        /// <summary>
        /// Mean estimated coordinates
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_MEAN_ESTIMATED_COORDINATES)]
        public Point2D MeanEstimatedCoords { get; set; }

        /// <summary>
        /// Accuracy
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_ACCURACIES_DEGREES)]
        public Accuracy Accuracy { get; set; }

        /// <summary>
        /// Mean Error
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_MEAN_ERRORS_PIXELS)]
        public MeanError MeanError { get; set; }

        /// <summary>
        /// Standard Deviation
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_STANDARD_DEVIATION_PIXELS)]
        public StandardDeviation StandardDeviation { get; set; }

        public CalibrationPoint()
        {
            Coordinates = new Point2D();
            MeanEstimatedCoords = new Point2D();
            Accuracy = new Accuracy();
            MeanError = new MeanError();
            StandardDeviation = new StandardDeviation();
        }
        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is CalibrationPoint))
                return false;

            var other = o as CalibrationPoint;

            return
                this.State == other.State &&
                Coordinates.Equals(other.Coordinates) &&
                MeanEstimatedCoords.Equals(other.MeanEstimatedCoords) &&
                Accuracy.Equals(other.Accuracy) &&
                MeanError.Equals(other.MeanError) &&
                StandardDeviation.Equals(other.StandardDeviation);
        }

        public override int GetHashCode()
        {
            int hash = 157;
            hash = hash * 953 + State.GetHashCode();
            hash = hash * 953 + Coordinates.GetHashCode();
            hash = hash * 953 + MeanEstimatedCoords.GetHashCode();
            hash = hash * 953 + Accuracy.GetHashCode();
            hash = hash * 953 + MeanError.GetHashCode();
            hash = hash * 953 + StandardDeviation.GetHashCode();
            return hash;
        }
    }
}