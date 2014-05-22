/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;

namespace TETCSharpClient.Reply
{
    internal class TrackerGetReply : ReplyBase
    {
        public TrackerGetReply()
        {
            Values = new TrackerGetReplyValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public TrackerGetReplyValues Values { set; get; }
    }
}
