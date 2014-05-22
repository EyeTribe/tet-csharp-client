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
    internal class RequestBase
    {
        public RequestBase()
        {
        }

        public RequestBase(string category, string request)
        {
            Category = category;
            Request = request;
        }

        [JsonProperty(PropertyName = Protocol.KEY_CATEGORY)]
        public string Category { set; get; }

        [JsonProperty(PropertyName = Protocol.KEY_REQUEST)]
        public string Request { set; get; }
    }
}
