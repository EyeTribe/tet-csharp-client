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
    internal class TrackerSetRequest : RequestBase
    {
        public TrackerSetRequest()
            : base(Protocol.CATEGORY_TRACKER, Protocol.TRACKER_REQUEST_SET)
        {
            Values = new TrackerSetRequestValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public TrackerSetRequestValues Values { set; get; }
    }
}
