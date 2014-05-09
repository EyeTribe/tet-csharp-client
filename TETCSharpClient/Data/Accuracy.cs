using Newtonsoft.Json;
using System;

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

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is Accuracy))
                return false;

            var other = o as Accuracy;

            return Double.Equals(Average, other.Average) &&
                Double.Equals(Left, other.Left) &&
                Double.Equals(Right, other.Right);
        }
    }
}