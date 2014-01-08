using Newtonsoft.Json;

namespace TETCSharpClient.Reply
{
    internal class ReplyFailed : ReplyBase
    {
        public ReplyFailed()
        {
            Values = new ReplyFailedValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public ReplyFailedValues Values { set; get; }
    }

}
