/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;
using EyeTribe.ClientSdk.Request;

namespace EyeTribe.ClientSdk.Response
{
    public class CalibrationPointEndResponse : ResponseBase
    {
        public CalibrationPointEndResponse() : base()
        {
            Values = new CalibrationPointEndResponseValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationPointEndResponseValues Values { set; get; }
    }
}
