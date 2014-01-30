using Newtonsoft.Json;

namespace TETCSharpClient.Reply
{
    internal class ReplyFailedValues
    {
        [JsonProperty(PropertyName = Protocol.KEY_STATUSMESSAGE)]
        public string StatusMessage { set; get; }
    }
}
