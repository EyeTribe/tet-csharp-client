using Newtonsoft.Json;
using System;

namespace TETCSharpClient.Data
{
    public class MeanError
    {
        /// <summary>
        /// Mean error in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_MEAN_ERROR_AVERAGE_PIXELS)]
        public double Average { get; set; }

        /// <summary>
        /// Mean error in pixels, left eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_MEAN_ERROR_LEFT_PIXELS)]
        public double Left { get; set; }

        /// <summary>
        /// Mean error in pixels, right eye
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_MEAN_ERROR_RIGHT_PIXELS)]
        public double Right { get; set; }

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is MeanError))
                return false;

            var other = o as MeanError;

            return Double.Equals(Average, other.Average) &&
                Double.Equals(Left, other.Left) &&
                Double.Equals(Right, other.Right);
        }
    }
}
