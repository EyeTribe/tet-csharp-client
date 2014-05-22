/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using Newtonsoft.Json;
using TETCSharpClient.Request;

namespace TETCSharpClient.Reply
{
    internal class CalibrationPointEndReply : ReplyBase
    {
        public CalibrationPointEndReply()
        {
            Values = new CalibrationPointEndReplyValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationPointEndReplyValues Values { set; get; }
    }
}
