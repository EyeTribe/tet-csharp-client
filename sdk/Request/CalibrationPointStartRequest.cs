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
    internal class CalibrationPointStartRequest : RequestBase<ResponseBase>
    {
        public CalibrationPointStartRequest(int x, int y) : base()
        {
            Values = new CalibrationPointStartRequestValues(x, y);

            this.Category = Protocol.CATEGORY_CALIBRATION;
            this.Request = Protocol.CALIBRATION_REQUEST_POINTSTART;
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationPointStartRequestValues Values { set; get; }
    }

    internal class CalibrationPointStartRequestValues
    {
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
