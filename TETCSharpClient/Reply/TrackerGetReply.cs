using Newtonsoft.Json;

namespace TETCSharpClient.Reply
{
    internal class TrackerGetReply : ReplyBase
    {
        public TrackerGetReply()
        {
            Values = new TrackerGetReplyValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public TrackerGetReplyValues Values { set; get; }
    }
}
