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
    internal class CalibrationPointStartRequestValues
    {
        public CalibrationPointStartRequestValues()
        {
        }

        public CalibrationPointStartRequestValues(int x, int y)
        {
            X = x;
            Y = y;
        }

        [JsonProperty(PropertyName = Protocol.CALIBRATION_X)]
        public int X { set; get; }

        [JsonProperty(PropertyName = Protocol.CALIBRATION_Y)]
        public int Y { set; get; }
    }
}
