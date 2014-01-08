using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    internal class CalibrationStartRequestValues
    {
        public CalibrationStartRequestValues()
        {
            PointCount = 9; // default number of calibration points
        }

        public CalibrationStartRequestValues(int numPoints)
        {
            PointCount = numPoints;
        }

        [JsonProperty(PropertyName = Protocol.CALIBRATION_POINT_COUNT)]
        public int PointCount { set; get; }
    }
}
