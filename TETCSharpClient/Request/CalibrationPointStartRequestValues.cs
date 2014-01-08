using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    internal class CalibrationPointStartRequestValues
    {
        public CalibrationPointStartRequestValues()
        {
        }

        public CalibrationPointStartRequestValues(int x, int y)
        {
            X = x;
            Y = y;
        }

        [JsonProperty(PropertyName = Protocol.CALIBRATION_X)]
        public int X { set; get; }

        [JsonProperty(PropertyName = Protocol.CALIBRATION_Y)]
        public int Y { set; get; }
    }
}
