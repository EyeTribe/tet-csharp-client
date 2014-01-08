using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    public class GazeMeanError
    {
        /// <summary>
        /// Mean error in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_MEAN_ERROR_AVERAGE_PIXELS)]
        public double MeanErrorPixels { get; set; }

        /// <summary>
        /// Mean error in pixels, left eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_MEAN_ERROR_LEFT_PIXELS)]
        public double MeanErrorPixelsLeft { get; set; }

        /// <summary>
        /// Mean error in pixels, right eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_MEAN_ERROR_RIGHT_PIXELS)]
        public double MeanErrorPixelsRight { get; set; }
    }
}
