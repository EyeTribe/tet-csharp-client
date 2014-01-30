using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    internal class TrackerGetRequest : RequestBase
    {
        public TrackerGetRequest()
            : base(Protocol.CATEGORY_TRACKER, Protocol.TRACKER_REQUEST_GET)
        {
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public string[] Values { set; get; }
    }

}
