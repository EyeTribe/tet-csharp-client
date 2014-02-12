using Newtonsoft.Json;
using TETCSharpClient.Data;

namespace TETCSharpClient.Request
{
    internal class CalibrationPointEndReplyValues
    {
        [JsonProperty(PropertyName = Protocol.CALIBRATION_CALIBRESULT)]
        public CalibrationResult CalibrationResult { set; get; }
    }
}
