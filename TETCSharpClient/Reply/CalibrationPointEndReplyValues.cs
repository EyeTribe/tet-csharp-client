/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;
using TETCSharpClient.Data;

namespace TETCSharpClient.Request
{
    internal class CalibrationPointEndReplyValues
    {
        [JsonProperty(PropertyName = Protocol.CALIBRATION_CALIBRESULT)]
        public CalibrationResult CalibrationResult { set; get; }
    }
}
