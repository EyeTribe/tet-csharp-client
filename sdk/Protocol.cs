/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

namespace EyeTribe.ClientSdk
{
    internal class Protocol
    {
        private Protocol() 
        {
            //ensure non-instantiability
        }

        public const int STATUSCODE_CALIBRATION_UPDATE = 800;
        public const int STATUSCODE_SCREEN_UPDATE = 801;
        public const int STATUSCODE_TRACKER_UPDATE = 802;

        public const string KEY_CATEGORY = "category";
        public const string KEY_REQUEST = "request";
        public const string KEY_ID = "id";
        public const string KEY_VALUES = "values";
        public const string KEY_STATUSCODE = "statuscode";
        public const string KEY_STATUSMESSAGE = "statusmessage";

        public const string CATEGORY_TRACKER = "tracker";
        public const string CATEGORY_CALIBRATION = "calibration";
        //public const string CATEGORY_HEARTBEAT = "heartbeat"; //deprecated

        public const string TRACKER_REQUEST_SET = "set";
        public const string TRACKER_REQUEST_GET = "get";
        //public const string TRACKER_MODE_PUSH = "push"; //deprecated
        //public const string TRACKER_HEARTBEATINTERVAL = "heartbeatinterval"; //deprecated
        public const string TRACKER_VERSION = "version";
        public const string TRACKER_ISCALIBRATED = "iscalibrated";
        public const string TRACKER_ISCALIBRATING = "iscalibrating";
        public const string TRACKER_TRACKERSTATE = "trackerstate";
        public const string TRACKER_CALIBRATIONRESULT = "calibresult";
        public const string TRACKER_FRAMERATE = "framerate";
        public const string TRACKER_FRAME = "frame";
        public const string TRACKER_SCREEN_INDEX = "screenindex";
        public const string TRACKER_SCREEN_RESOLUTION_WIDTH = "screenresw";
        public const string TRACKER_SCREEN_RESOLUTION_HEIGHT = "screenresh";
        public const string TRACKER_SCREEN_PHYSICAL_WIDTH = "screenpsyw";
        public const string TRACKER_SCREEN_PHYSICAL_HEIGHT = "screenpsyh";

        public const string CALIBRATION_REQUEST_START = "start";
        public const string CALIBRATION_REQUEST_ABORT = "abort";
        public const string CALIBRATION_REQUEST_POINTSTART = "pointstart";
        public const string CALIBRATION_REQUEST_POINTEND = "pointend";
        public const string CALIBRATION_REQUEST_CLEAR = "clear";
        public const string CALIBRATION_CALIBRESULT = "calibresult";
        public const string CALIBRATION_CALIBPOINTS = "calibpoints";
        public const string CALIBRATION_POINT_COUNT = "pointcount";
        public const string CALIBRATION_X = "x";
        public const string CALIBRATION_Y = "y";

        public const string FRAME_TIME = "time";
        public const string FRAME_TIMESTAMP = "timeStamp";
        public const string FRAME_FIXATION = "fix";
        public const string FRAME_STATE = "state";
        public const string FRAME_RAW_COORDINATES = "raw";
        public const string FRAME_AVERAGE_COORDINATES = "avg";
        public const string FRAME_X = "x";
        public const string FRAME_Y = "y";
        public const string FRAME_LEFT_EYE = "lefteye";
        public const string FRAME_RIGHT_EYE = "righteye";
        public const string FRAME_EYE_PUPIL_SIZE = "psize";
        public const string FRAME_EYE_PUPIL_CENTER = "pcenter";

        public const string CALIBRESULT_RESULT = "result";
        public const string CALIBRESULT_AVERAGE_ERROR_DEGREES = "deg";
        public const string CALIBRESULT_AVERAGE_ERROR_LEFT_DEGREES = "degl";
        public const string CALIBRESULT_AVERAGE_ERROR_RIGHT_DEGREES = "degr";
        public const string CALIBRESULT_CALIBRATION_POINTS = "calibpoints";
        public const string CALIBRESULT_STATE = "state";
        public const string CALIBRESULT_COORDINATES = "cp";
        public const string CALIBRESULT_X = "x";
        public const string CALIBRESULT_Y = "y";
        public const string CALIBRESULT_MEAN_ESTIMATED_COORDINATES = "mecp";
        public const string CALIBRESULT_ACCURACIES_DEGREES = "acd";
        public const string CALIBRESULT_ACCURACY_AVERAGE_DEGREES = "ad";
        public const string CALIBRESULT_ACCURACY_LEFT_DEGREES = "adl";
        public const string CALIBRESULT_ACCURACY_RIGHT_DEGREES = "adr";
        public const string CALIBRESULT_MEAN_ERRORS_PIXELS = "mepix";
        public const string CALIBRESULT_MEAN_ERROR_AVERAGE_PIXELS = "mep";
        public const string CALIBRESULT_MEAN_ERROR_LEFT_PIXELS = "mepl";
        public const string CALIBRESULT_MEAN_ERROR_RIGHT_PIXELS = "mepr";
        public const string CALIBRESULT_STANDARD_DEVIATION_PIXELS = "asdp";
        public const string CALIBRESULT_STANDARD_DEVIATION_AVERAGE_PIXELS = "asd";
        public const string CALIBRESULT_STANDARD_DEVIATION_LEFT_PIXELS = "asdl";
        public const string CALIBRESULT_STANDARD_DEVIATION_RIGHT_PIXELS = "asdr";
    }
}
