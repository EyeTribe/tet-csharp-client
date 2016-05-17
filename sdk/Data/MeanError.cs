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

            return Double.Equals(this.Average, other.Average) &&
                Double.Equals(this.Left, other.Left) &&
                Double.Equals(this.Right, other.Right);
        }

        public override int GetHashCode()
        {
            int hash = 3;
            hash = hash * 37 + Average.GetHashCode();
            hash = hash * 37 + Left.GetHashCode();
            hash = hash * 37 + Right.GetHashCode();
            return hash;
        }
    }
}
