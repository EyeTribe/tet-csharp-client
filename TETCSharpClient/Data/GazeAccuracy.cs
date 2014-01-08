using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    public class GazeAccuracy
    {
        /// <summary>
        /// Accuracy in degrees
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_ACCURACY_AVERAGE_DEGREES)]
        public double AccuracyDegrees { get; set; }

        /// <summary>
        /// Accuracy in degrees, left eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_ACCURACY_LEFT_DEGREES)]
        public double AccuracyDegreesLeft { get; set; }

        /// <summary>
        /// Accuracy in degrees, right eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_ACCURACY_RIGHT_DEGREES)]
        public double AccuracyDegreesRight { get; set; }

    }

}