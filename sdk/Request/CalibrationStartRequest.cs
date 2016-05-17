/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;
using EyeTribe.ClientSdk.Response;

namespace EyeTribe.ClientSdk.Request
{
    internal class CalibrationStartRequest : RequestBase<ResponseBase>
    {
        public CalibrationStartRequest(int numPoints) : base()
        {
            Values = new CalibrationStartRequestValues(numPoints);

            this.Category = Protocol.CATEGORY_CALIBRATION;
            this.Request = Protocol.CALIBRATION_REQUEST_START;
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationStartRequestValues Values { set; get; }
    }

}
