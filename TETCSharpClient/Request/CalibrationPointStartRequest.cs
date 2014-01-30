using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    internal class CalibrationPointStartRequest : RequestBase
    {
        public CalibrationPointStartRequest()
            : base(Protocol.CATEGORY_CALIBRATION, Protocol.CALIBRATION_REQUEST_POINTSTART)
        {
            Values = new CalibrationPointStartRequestValues();
        }

        public CalibrationPointStartRequest(int x, int y)
            : base(Protocol.CATEGORY_CALIBRATION, Protocol.CALIBRATION_REQUEST_POINTSTART)
        {
            Values = new CalibrationPointStartRequestValues(x, y);
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationPointStartRequestValues Values { set; get; }
    }
}
