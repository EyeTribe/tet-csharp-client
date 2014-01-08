using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    internal class CalibrationStartRequest : RequestBase
    {
        public CalibrationStartRequest()
            : base(Protocol.CATEGORY_CALIBRATION, Protocol.CALIBRATION_REQUEST_START)
        {
            Values = new CalibrationStartRequestValues();
        }

        public CalibrationStartRequest(int numPoints)
            : base(Protocol.CATEGORY_CALIBRATION, Protocol.CALIBRATION_REQUEST_START)
        {
            Values = new CalibrationStartRequestValues(numPoints); 
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationStartRequestValues Values { set; get; }
    }

}
