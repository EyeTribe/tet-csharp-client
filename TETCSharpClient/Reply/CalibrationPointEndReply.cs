using Newtonsoft.Json;
using TETCSharpClient.Request;

namespace TETCSharpClient.Reply
{
    internal class CalibrationPointEndReply : ReplyBase
    {
        public CalibrationPointEndReply()
        {
            Values = new CalibrationPointEndReplyValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public CalibrationPointEndReplyValues Values { set; get; }
    }
}
