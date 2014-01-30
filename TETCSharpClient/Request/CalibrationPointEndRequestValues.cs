using Newtonsoft.Json;
using TETCSharpClient.Data;

namespace TETCSharpClient.Request
{
    internal class CalibrationPointEndRequestValues
    {
        [JsonProperty(PropertyName = Protocol.CALIBRATION_CALIBRESULT)]
        public CalibrationResult CalibrationResult { set; get; }
    }
}
