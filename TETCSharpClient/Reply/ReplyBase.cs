using Newtonsoft.Json;

namespace TETCSharpClient.Reply
{
    public class ReplyBase
    {
        [JsonProperty(PropertyName = Protocol.KEY_CATEGORY)]
        public string Category { set; get; }

        [JsonProperty(PropertyName = Protocol.KEY_REQUEST)]
        public string Request { set; get; }

        [JsonProperty(PropertyName = Protocol.KEY_STATUSCODE)]
        public int StatusCode { set; get; }
    }

}
