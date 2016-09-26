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
    internal class TrackerGetResponse : ResponseBase
    {
        public TrackerGetResponse() : base()
        {
            Values = new TrackerGetResponseValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public TrackerGetResponseValues Values { set; get; }
    }
}
