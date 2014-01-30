using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    public class Accuracy
    {
        /// <summary>
        /// Accuracy in degrees
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_ACCURACY_AVERAGE_DEGREES)]
        public double Average { get; set; }

        /// <summary>
        /// Accuracy in degrees, left eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_ACCURACY_LEFT_DEGREES)]
        public double Left { get; set; }

        /// <summary>
        /// Accuracy in degrees, right eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_ACCURACY_RIGHT_DEGREES)]
        public double Right { get; set; }
    }
}