/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using Newtonsoft.Json;

namespace EyeTribe.ClientSdk.Data
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

            return
                Double.Equals(this.Average, other.Average) &&
                Double.Equals(this.Left, other.Left) &&
                Double.Equals(this.Right, other.Right);
        }

        public override int GetHashCode()
        {
            int hash = 7;
            hash = hash * 29 + Average.GetHashCode();
            hash = hash * 29 + Left.GetHashCode();
            hash = hash * 29 + Right.GetHashCode();
            return hash;
        }
    }
}