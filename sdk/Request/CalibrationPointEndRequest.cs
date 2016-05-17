/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EyeTribe.ClientSdk.Response;

namespace EyeTribe.ClientSdk.Request
{
    internal class CalibrationPointEndRequest : RequestBase<CalibrationPointEndResponse>
    {
        public CalibrationPointEndRequest() : base()
        {
            this.Category = Protocol.CATEGORY_CALIBRATION;
            this.Request = Protocol.CALIBRATION_REQUEST_POINTEND;
        }

        public override object ParseJsonResponse(JObject response)
        {
            CalibrationPointEndResponse cper = (CalibrationPointEndResponse)base.ParseJsonResponse(response);

            JToken value;
            if (!response.TryGetValue(Protocol.CALIBRATION_CALIBRESULT, out value))
                cper.Values.CalibrationResult = null;

            return cper;
        }
    }
}
