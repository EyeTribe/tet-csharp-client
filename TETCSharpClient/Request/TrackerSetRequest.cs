using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    internal class TrackerSetRequest : RequestBase
    {
        public TrackerSetRequest()
            : base(Protocol.CATEGORY_TRACKER, Protocol.TRACKER_REQUEST_SET)
        {
            Values = new TrackerSetRequestValues();
        }

        [JsonProperty(PropertyName = Protocol.KEY_VALUES)]
        public TrackerSetRequestValues Values { set; get; }
    }
}
