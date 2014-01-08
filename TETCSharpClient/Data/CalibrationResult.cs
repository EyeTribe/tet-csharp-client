using System;
using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    /// <summary>
    /// GazeCalibrationResult holds outcome of a calibration procedure. It defines if
    /// calibration was successful or if certain calibration points needs resampling.
    /// </summary>
    public class CalibrationResult
    {
        /// <summary>
        /// Was the calibration sucessful?
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_RESULT)]
        public bool Result { get; set; }

        /// <summary>
        /// average error in degrees
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_AVERAGE_ERROR_DEGREES)]
        public double AverageErrorDegree { get; set; }

        /// <summary>
        /// average error in degs, left eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_AVERAGE_ERROR_LEFT_DEGREES)]
        public double AverageErrorDegreeLeft { get; set; }

        /// <summary>
        /// average error in degs, right eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_AVERAGE_ERROR_RIGHT_DEGREES)]
        public double AverageErrorDegreeRight { get; set; }

        /// <summary>
        /// complete list of calibrationpoints
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_CALIBRATION_POINTS)]
        public CalibrationPoint[] Calibpoints { get; set; }

        public CalibrationResult()
        {
            Result = false;
            AverageErrorDegree = 0d;
            AverageErrorDegreeLeft = 0d;
            AverageErrorDegreeRight = 0d;
            Calibpoints = new CalibrationPoint[0];
        }

        public CalibrationResult(CalibrationResult other)
        {
            Result = other.Result;
            AverageErrorDegree = other.AverageErrorDegree;
            AverageErrorDegreeLeft = other.AverageErrorDegreeLeft;
            AverageErrorDegreeRight = other.AverageErrorDegreeRight;
            Calibpoints = (CalibrationPoint[]) other.Calibpoints.Clone();
        }

        public CalibrationResult(String json)
        {
            Set(JsonConvert.DeserializeObject<CalibrationResult>(json));
        }

        private void Set(CalibrationResult other)
        {
            Result = other.Result;
            AverageErrorDegree = other.AverageErrorDegree;
            AverageErrorDegreeLeft = other.AverageErrorDegreeLeft;
            AverageErrorDegreeRight = other.AverageErrorDegreeRight;
            Calibpoints = other.Calibpoints;
        }

        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}