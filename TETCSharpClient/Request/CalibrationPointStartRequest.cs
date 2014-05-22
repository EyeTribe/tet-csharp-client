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
    internal class CalibrationPointStartRequest : RequestBase
    {
        public CalibrationPointStartRequest()
            : base(Protocol.CATEGORY_CALIBRATION, Protocol.CALIBRATION_REQUEST_POINTSTART)
        {
            Values = new CalibrationPointStartRequestValues();
        }

        public CalibrationPointStartRequest(int x, int y)
            : base(Protocol.CATEGORY_CALIBRATION, Protocol.CALIBRATION_REQUEST_POINTSTART)
        {
            Values = new CalibrationPointStartRequestValues(x, y);
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationPointStartRequestValues Values { set; get; }
    }
}
