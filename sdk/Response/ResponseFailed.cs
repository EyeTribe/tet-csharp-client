/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;

namespace EyeTribe.ClientSdk.Response
{
    internal class ResponseFailed : ResponseBase
    {
        public ResponseFailed()
        {
            Values = new ResponseFailedValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public ResponseFailedValues Values { set; get; }
    }
}
