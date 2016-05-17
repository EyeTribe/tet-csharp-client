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

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is StandardDeviation))
                return false;

            var other = o as StandardDeviation;

            return
                Double.Equals(this.Average, other.Average) &&
                Double.Equals(this.Left, other.Left) &&
                Double.Equals(this.Right, other.Right);
        }

        public override int GetHashCode()
        {
            int hash = 3;
            hash = hash * 19 + Average.GetHashCode();
            hash = hash * 19 + Left.GetHashCode();
            hash = hash * 19 + Right.GetHashCode();
            return hash;
        }
    }
}