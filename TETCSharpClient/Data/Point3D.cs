/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
namespace TETCSharpClient.Data
{
    public class Point3D : Point2D
    {
        private double z;

        public Point3D(double x, double y, double z)
        {
            base.X = x;
            base.Y = y;
            this.z = z;
        }

        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is Point3D))
                return false;

            var other = o as Point3D;

            return base.Equals(o) && Double.Equals(z, other.z);
        }
    }
}
