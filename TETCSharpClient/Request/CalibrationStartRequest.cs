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
    internal class CalibrationStartRequest : RequestBase
    {
        public CalibrationStartRequest()
            : base(Protocol.CATEGORY_CALIBRATION, Protocol.CALIBRATION_REQUEST_START)
        {
            Values = new CalibrationStartRequestValues();
        }

        public CalibrationStartRequest(int numPoints)
            : base(Protocol.CATEGORY_CALIBRATION, Protocol.CALIBRATION_REQUEST_START)
        {
            Values = new CalibrationStartRequestValues(numPoints);
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationStartRequestValues Values { set; get; }
    }

}
