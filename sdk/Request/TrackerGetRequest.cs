/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EyeTribe.ClientSdk.Data;
using EyeTribe.ClientSdk.Response;

namespace EyeTribe.ClientSdk.Request
{
    internal class TrackerGetRequest : RequestBase<TrackerGetResponse>
    {
        public TrackerGetRequest()
        {
            this.Category = Protocol.CATEGORY_TRACKER;
            this.Request = Protocol.TRACKER_REQUEST_GET;
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public string[] Values { set; get; }

        public override object ParseJsonResponse(JObject response)
        {
            TrackerGetResponse tgr = (TrackerGetResponse)base.ParseJsonResponse(response);

            if (null != tgr.Values.Frame)
            {
                // fixing timestamp based on string
                // representation, Json 32bit int issue
                // TODO: This will eventually be done serverside

                if (!String.IsNullOrEmpty(tgr.Values.Frame.TimeStampString))
                {
                    DateTime gdTime = DateTime.ParseExact(tgr.Values.Frame.TimeStampString, GazeData.TIMESTAMP_STRING_FORMAT,
                    System.Globalization.CultureInfo.InvariantCulture);
                    tgr.Values.Frame.TimeStamp = (long)((double)gdTime.Ticks / TimeSpan.TicksPerMillisecond);
                }
            }

            return tgr;
        }
    }
}
