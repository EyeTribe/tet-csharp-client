/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TETCSharpClient.Data
{
    /// <summary>
    /// CalibrationResult holds outcome of a calibration procedure. It defines if
    /// calibration was successful or if certain calibration points needs resampling.
    /// </summary>
    public class CalibrationResult
    {
        /// <summary>
        /// Was the calibration sucessful?
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_RESULT)]
        public bool Result { get; set; }

        /// <summary>
        /// average error in degrees
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_AVERAGE_ERROR_DEGREES)]
        public double AverageErrorDegree { get; set; }

        /// <summary>
        /// average error in degs, left eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_AVERAGE_ERROR_LEFT_DEGREES)]
        public double AverageErrorDegreeLeft { get; set; }

        /// <summary>
        /// average error in degs, right eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_AVERAGE_ERROR_RIGHT_DEGREES)]
        public double AverageErrorDegreeRight { get; set; }

        /// <summary>
        /// complete list of calibrationpoints
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_CALIBRATION_POINTS)]
        public CalibrationPoint[] Calibpoints { get; set; }

        public CalibrationResult()
        {
            Result = false;
            AverageErrorDegree = 0d;
            AverageErrorDegreeLeft = 0d;
            AverageErrorDegreeRight = 0d;
            Calibpoints = new CalibrationPoint[0];
        }

        public CalibrationResult(CalibrationResult other)
        {
            Result = other.Result;
            AverageErrorDegree = other.AverageErrorDegree;
            AverageErrorDegreeLeft = other.AverageErrorDegreeLeft;
            AverageErrorDegreeRight = other.AverageErrorDegreeRight;
            Calibpoints = (CalibrationPoint[])other.Calibpoints.Clone();
        }

        public CalibrationResult(String json)
        {
            Set(JsonConvert.DeserializeObject<CalibrationResult>(json));
        }

        private void Set(CalibrationResult other)
        {
            Result = other.Result;
            AverageErrorDegree = other.AverageErrorDegree;
            AverageErrorDegreeLeft = other.AverageErrorDegreeLeft;
            AverageErrorDegreeRight = other.AverageErrorDegreeRight;
            Calibpoints = other.Calibpoints;
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is CalibrationResult))
                return false;

            var other = o as CalibrationResult;

            return
                this.Result == other.Result &&
                Double.Equals(this.AverageErrorDegree, other.AverageErrorDegree) &&
                Double.Equals(this.AverageErrorDegreeLeft, other.AverageErrorDegreeLeft) &&
                Double.Equals(this.AverageErrorDegreeRight, other.AverageErrorDegreeRight) &&
                ArraysEqual(this.Calibpoints, other.Calibpoints);
        }

        private bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
                if (!comparer.Equals(a1[i], a2[i]))
                    return false;
            return true;
        }
    }
}