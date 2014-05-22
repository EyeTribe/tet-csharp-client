/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System.ComponentModel;
using Newtonsoft.Json;
using TETCSharpClient.Data;

namespace TETCSharpClient.Reply
{
    internal class TrackerGetReplyValues
    {
        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_MODE_PUSH)]
        public bool? Push { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_HEARTBEATINTERVAL)]
        public int? HeartbeatInterval { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_VERSION)]
        public int? Version { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_TRACKERSTATE)]
        public int? TrackerState { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_ISCALIBRATING)]
        public bool? IsCalibrating { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_ISCALIBRATED)]
        public bool? IsCalibrated { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_CALIBRATIONRESULT)]
        public CalibrationResult CalibrationResult { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_FRAMERATE)]
        public int? FrameRate { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_INDEX)]
        public int? ScreenIndex { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_RESOLUTION_WIDTH)]
        public int? ScreenResolutionWidth { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_RESOLUTION_HEIGHT)]
        public int? ScreenResolutionHeight { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_PHYSICAL_WIDTH)]
        public float? ScreenPhysicalWidth { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_PHYSICAL_HEIGHT)]
        public float? ScreenPhysicalHeight { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_FRAME)]
        public GazeData Frame { set; get; }
    }
}
