using Newtonsoft.Json;
using TETCSharpClient.Request;

namespace TETCSharpClient.Reply
{
    internal class CalibrationPointEndReply : ReplyBase
    {
        public CalibrationPointEndReply()
        {
            Values = new CalibrationPointEndRequestValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationPointEndRequestValues Values { set; get; }
    }
}
