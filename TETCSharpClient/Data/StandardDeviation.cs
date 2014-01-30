using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    public class StandardDeviation
    {
        /// <summary>
        /// Average standard deviation, in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_STANDARD_DEVIATION_AVERAGE_PIXELS)]
        public double Average { get; set; }

        /// <summary>
        /// Left eye standard deviation, in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_STANDARD_DEVIATION_LEFT_PIXELS)]
        public double Left { get; set; }

        /// <summary>
        /// Right eye standard deviation, in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_STANDARD_DEVIATION_RIGHT_PIXELS)]
        public double Right { get; set; }
    }
}