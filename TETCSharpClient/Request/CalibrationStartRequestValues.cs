/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    internal class CalibrationStartRequestValues
    {
        public CalibrationStartRequestValues()
        {
            PointCount = 9; // default number of calibration points
        }

        public CalibrationStartRequestValues(int numPoints)
        {
            PointCount = numPoints;
        }

        [JsonProperty(PropertyName = Protocol.CALIBRATION_POINT_COUNT)]
        public int PointCount { set; get; }
    }
}
