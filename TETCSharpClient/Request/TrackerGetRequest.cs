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
    internal class TrackerGetRequest : RequestBase
    {
        public TrackerGetRequest()
            : base(Protocol.CATEGORY_TRACKER, Protocol.TRACKER_REQUEST_GET)
        {
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public string[] Values { set; get; }
    }

}
