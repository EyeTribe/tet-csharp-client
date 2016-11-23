/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using EyeTribe.ClientSdk.Data;
using Newtonsoft.Json;
using System.ComponentModel;

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

    internal class TrackerGetResponseValues
    {
        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_VERSION)]
        public int? Version { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_TRACKERSTATE)]
        public int? TrackerState { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_IS_CALIBRATING)]
        public bool? IsCalibrating { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_IS_CALIBRATED)]
        public bool? IsCalibrated { set; get; }

        [DefaultValue(null)]
        [JsonProperty(PropertyName = Protocol.TRACKER_CALIBRATION_RESULT)]
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
