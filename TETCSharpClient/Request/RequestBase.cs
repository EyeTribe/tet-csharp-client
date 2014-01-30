using Newtonsoft.Json;

namespace TETCSharpClient.Request
{
    public class RequestBase
    {
        public RequestBase()
        {
        }

        public RequestBase(string category, string request)
        {
            Category = category;
            Request = request;
        }

        [JsonProperty(PropertyName = Protocol.KEY_CATEGORY)]
        public string Category { set; get; }

        [JsonProperty(PropertyName = Protocol.KEY_REQUEST)]
        public string Request { set; get; }
    }
}
