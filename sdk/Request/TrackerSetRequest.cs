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
    internal class TrackerSetRequest : RequestBase<ResponseBase>
    {
        public TrackerSetRequest()
        {
            Values = new TrackerSetRequestValues();

            this.Category = Protocol.CATEGORY_TRACKER;
            this.Request = Protocol.TRACKER_REQUEST_SET;
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public TrackerSetRequestValues Values { set; get; }
    }
}
