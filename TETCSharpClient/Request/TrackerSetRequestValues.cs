using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    internal class TrackerSetRequestValues
    {
        [JsonProperty(PropertyName = Protocol.TRACKER_MODE_PUSH, NullValueHandling = NullValueHandling.Ignore)]
        public bool? Push { set; get; }

        [JsonProperty(PropertyName = Protocol.TRACKER_VERSION, NullValueHandling = NullValueHandling.Ignore)]
        public int? Version { set; get; }

        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_INDEX, NullValueHandling = NullValueHandling.Ignore)]
        public int? ScreenIndex { set; get; }

        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_RESOLUTION_WIDTH, NullValueHandling = NullValueHandling.Ignore)]
        public int? ScreenResolutionWidth { set; get; }

        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_RESOLUTION_HEIGHT, NullValueHandling = NullValueHandling.Ignore)]
        public int? ScreenResolutionHeight { set; get; }

        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_PHYSICAL_WIDTH, NullValueHandling = NullValueHandling.Ignore)]
        public float? ScreenPhysicalWidth { set; get; }

        [JsonProperty(PropertyName = Protocol.TRACKER_SCREEN_PHYSICAL_HEIGHT, NullValueHandling = NullValueHandling.Ignore)]
        public float? ScreenPhysicalHeight { set; get; }
    }
}
