using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    public class GazeStandardDeviation
    {
        /// <summary>
        /// Average std deviation in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_STANDARD_DEVIATION_AVERAGE_PIXELS)]
        public double AverageStandardDeviataionPixels { get; set; }

        /// <summary>
        /// Avg std deviation in pix, left eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_STANDARD_DEVIATION_LEFT_PIXELS)]
        public double AverageStandardDeviataionPixelsLeft { get; set; }

        /// <summary>
        /// Avg std deviation in pix, right eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_STANDARD_DEVIATION_RIGHT_PIXELS)]
        public double AverageStandardDeviataionPixelsRight { get; set; }
    }
}